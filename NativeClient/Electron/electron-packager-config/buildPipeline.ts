import { normalize } from 'path';
import { spawn } from 'child_process';
import * as fs from 'fs-extra';

export class BuildPipeline {
    private DEBUG_BUILD_PIPELINE = false;
    private electronProjectPath = normalize(__dirname + '/../../');
    private componentsBuildPath = normalize(this.electronProjectPath + '../');
    private npmCommand = process.platform === 'win32' ? 'npm.cmd' : 'npm';
    private emptyDatabaseFilename = '__empty__.sqlite';
    private webAPIAppDir = 'NativeClient.WebAPI';
    private webUIAppDir = 'NativeClient.WebUI';
    private dotnetRuntimeID: string = null;

    constructor(platform: string, arch: string) {
        this.dotnetRuntimeID =
            this._packagerPlatformAndArchToDotnetRuntimeID(platform, arch);
    }

    private _packagerPlatformAndArchToDotnetRuntimeID(
        platform: string,
        arch: string
    ): string {
        const dotnetPlatform = new Map<string, string>([
            ['win32', 'win'],
            ['linux', 'linux'],
            ['darvin', 'osx']
        ]).get(platform);
        const dotnetArch = new Map<string, string>([
            ['ia32', 'x86'],
            ['x64', 'x64']
        ]).get(arch);
        if (dotnetArch === undefined || dotnetPlatform === undefined
            || (dotnetArch === 'x86' && dotnetPlatform !== 'win')
        ) {
            throw new Error(`Wrong platform (${platform}) or arch (${arch}).`);
        } else {
            return dotnetPlatform+'-'+dotnetArch;
        }
    }

    private runConsoleCommand(cwd: string, cmd: string, args: string[]): Promise<number> {
        const normCwd = normalize(cwd);
        if (this.DEBUG_BUILD_PIPELINE === true) {
            const fullCmd = args.reduce((acc, next) => `${acc} ${next}`, cmd);
            console.log(`Running '${fullCmd}' in ${normCwd} folder`);
        }
        const execution = spawn(cmd, args, { cwd: normCwd });
        execution.stdout.pipe(process.stdout);
        execution.stderr.pipe(process.stderr);
        return new Promise<number>((resolve, reject) => {
            execution.on('close', (code) => resolve(code));
        });
    }

    private async isPathOK(path: string): Promise<boolean> {
        const norm = normalize(path);
        const exists = await fs.pathExists(norm);
        if (this.DEBUG_BUILD_PIPELINE === true) {
            if (exists === true) {
                console.log(`Path exists: ${norm}`);
            } else {
                console.log(`Path does not exist: ${norm}`);
            }
        }
        return exists;
    }

    private async isElectronAppBuilt(): Promise<boolean> {
        const electronBase = __dirname+'/../../app/js/';
        const result = await this.isPathOK(electronBase+'main.js')
        && await this.isPathOK(electronBase+'renderer.js');
        if (result === true) {
            console.log('Found built base Electron app project.');
        } else {
            console.log('Base electron app project is not built.');
        }
        return result;
    }

    private async isUIAppBuilt(): Promise<boolean> {
        const uiDistBase = this.componentsBuildPath+this.webUIAppDir+'/dist/';
        const result = await this.isPathOK(uiDistBase+'WebUI');
        if (result === true) {
            console.log('Found built UI project.');
        } else {
            console.log('UI project is not built.');
        }
        return result;
    }

    private async isAPIAppBuilt(): Promise<boolean> {
        const apiReleaseBinBase = this.componentsBuildPath+this.webAPIAppDir
        +'/bin/release/netcoreapp2.2/';
        const result = await this.isPathOK(
            apiReleaseBinBase+this.dotnetRuntimeID+'/publish'
        );
        if (result === true) {
            console.log('Found built API project.');
        } else {
            console.log('API project is not built.');
        }
        return result;
    }

    private buildElectronApp(): Promise<number> {
        console.log('Building base Electron app.');
        return this.runConsoleCommand(
            __dirname+'/../../',
            this.npmCommand,
            ['run', 'build']
        );
    }

    private buildWebUI(): Promise<number> {
        console.log('Building WebUI.');
        console.log('This may take some time...');
        const uiProjectDir = this.componentsBuildPath + this.webUIAppDir;
        return this.runConsoleCommand(
            uiProjectDir,
            this.npmCommand,
            ['run', 'build']
        );
    }

    private async buildWebAPI(): Promise<number> {
        console.log('Building WebAPI.');
        const apiProjectDir = this.componentsBuildPath + this.webAPIAppDir;
        return this.runConsoleCommand(
            apiProjectDir,
            'dotnet',
            ['publish', '-c', 'release', '-r', this.dotnetRuntimeID]
        );
    }

    private async buildEmptyAPIDatabase(forced = false): Promise<number> {
        const apiProjectDir = this.componentsBuildPath + this.webAPIAppDir;
        const createdEmpty = normalize(apiProjectDir+'/'+this.emptyDatabaseFilename);
        const exists = await this.isPathOK(createdEmpty);
        if (exists === true) {
            if (forced === true) {
                await fs.remove(createdEmpty);
            } else {
                console.log('Emtpy database already created.');
                return 0;
            }
        }

        console.log('Building empty database.');
        const createdDatabase = normalize(apiProjectDir+'/DB.sqlite');
        const tempRenamed = normalize(apiProjectDir+'/__temp_renamed__.sqlite');

        const someDatabaseCreated = this.isPathOK(createdDatabase);
        if (someDatabaseCreated) {
            await fs.rename(createdDatabase, tempRenamed);
        }
        const result = await this.runConsoleCommand(
            apiProjectDir,
            'dotnet',
            ['ef', 'database', 'update']
        );
        if (result === 0) { //created ok
            await fs.rename(createdDatabase, createdEmpty);
        }
        if (someDatabaseCreated) {
            await fs.rename(tempRenamed, createdDatabase);
        }
        return result;
    }

    async rebuildAllSubprojects(): Promise<boolean> {
        const electronBuildIsOK = (await this.buildElectronApp()) === 0;
        if (!electronBuildIsOK) { return false; }
        const apiBuildIsOK = (await this.buildWebAPI()) === 0;
        if (!apiBuildIsOK) { return false; }
        const uiBuildISOK = (await this.buildWebUI()) === 0;
        if (!uiBuildISOK) { return false; }
        return (await this.buildEmptyAPIDatabase(true)) === 0;
    }
    

    async buildAllUnbuiltSubprojects(): Promise<boolean> {
        const electron = await this.isElectronAppBuilt()
        || await this.buildElectronApp() === 0;
        if (electron === false) return false;
        const webUI = await this.isUIAppBuilt()
        || await this.buildWebUI() === 0;
        if (webUI === false) return false;
        const webAPI = await this.isAPIAppBuilt()
        || await this.buildWebAPI() === 0;
        if (webAPI === false) return false;
        return await this.buildEmptyAPIDatabase() === 0;
    }

    async copyWebUITo(path: string): Promise<void> {
        console.log('Copying WebUI dir to electron package...');
        await fs.copy(
            normalize(this.componentsBuildPath+this.webUIAppDir+'/dist/WebUI'),
            normalize(path+'/WebUI')
        );
        console.log('Finished copying WebUI dir.');
    }
    
    async copyWebAPITo(path: string): Promise<void> {
        console.log('Copying WebAPI dir to electron package...');
        const srcPath = this.componentsBuildPath+this.webAPIAppDir
        +'/bin/release/netcoreapp2.2/'+this.dotnetRuntimeID+'/publish';
        await fs.copy(
            normalize(srcPath),
            normalize(path+'/WebAPI')
        );
        console.log('Finished copying WebAPI dir.');
    }

    async copyAPIDatabaseTo(path: string): Promise<void> {
        console.log('Copying empty database to electron package...');
        const srcFile = normalize(this.componentsBuildPath+this.webAPIAppDir
        +'/'+this.emptyDatabaseFilename);
        const destFile = normalize(path+'/WebAPI/DB.sqlite');
        await fs.copy(srcFile, destFile);
        console.log('Finished copying empty database.');
    }
}