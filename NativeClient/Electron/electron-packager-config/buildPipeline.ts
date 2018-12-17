import { normalize } from 'path';
import { spawn } from 'child_process';
import * as fs from 'fs-extra';

export class BuildPipeline {
    private componentsBuildPath = __dirname + '../../../../';
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
        const execution = spawn(cmd, args, { cwd });
        execution.stdout.pipe(process.stdout);
        execution.stderr.pipe(process.stderr);
        return new Promise<number>((resolve, reject) => {
            execution.on('close', (code) => resolve(code));
        });
    }

    private isPathOK(path: string): Promise<boolean> {
        return fs.pathExists(normalize(path));
    }

    private async isElectronAppBuilt(): Promise<boolean> {
        const electronBase = __dirname+'../../../app/js/';
        return await this.isPathOK(electronBase+'main.js')
        && await this.isPathOK(electronBase+'renderer.js');
    }

    private async isUIAppBuilt(): Promise<boolean> {
        const uiDistBase = this.componentsBuildPath+this.webUIAppDir+'dist';
        return await this.isPathOK(uiDistBase+'WebUI');
    }

    private async isAPIAppBuilt(): Promise<boolean> {
        const apiReleaseBinBase = this.componentsBuildPath+this.webAPIAppDir
        +'/bin/release/netcoreapp2.2/';
        return await this.isPathOK(apiReleaseBinBase+this.dotnetRuntimeID+'/publish');
    }

    private buildElectronApp(): Promise<number> {
        console.log('Building ElectronApp');
        return this.runConsoleCommand(__dirname+'../../../', 'npm', ['run', 'build']);
    }

    private async buildWebAPI(): Promise<number> {
        console.log('Building WebAPI');
        const apiProjectDir = this.componentsBuildPath + this.webAPIAppDir;
        return this.runConsoleCommand(
            apiProjectDir,
            'dotnet',
            ['publish', '-c', 'release', '-r', this.dotnetRuntimeID]
        );
    }

    private buildWebUI(): Promise<number> {
        console.log('Building WebUI');
        const uiProjectDir = this.componentsBuildPath + this.webUIAppDir;
        return this.runConsoleCommand(
            uiProjectDir,
            'ng',
            ['build', '--prod', '--aot']
        );
    }

    async rebuildAllSubprojects(): Promise<boolean> {
        const electronBuildIsOK = (await this.buildElectronApp()) === 0;
        const apiBuildIsOK = electronBuildIsOK
        && (await this.buildWebAPI()) === 0;
        const uiBuildISOK = apiBuildIsOK && (await this.buildWebUI()) === 0;
        return uiBuildISOK;
    }
    

    async buildAllUnbuiltSubprojects(): Promise<boolean> {
        const electron = await this.isElectronAppBuilt()
        && await this.buildElectronApp() === 0;
        if (electron === false) return false;
        const webUI = await this.isUIAppBuilt()
        && await this.buildWebUI() === 0;
        if (webUI === false) return false;
        return await this.isAPIAppBuilt()
        && await this.buildWebAPI() === 0;
    }

    async copyWebUITo(path: string): Promise<void> {
        console.log('Copying WebUI dir to electron package...');
        await fs.copy(
            normalize(this.componentsBuildPath+this.webUIAppDir+'dist/WebUI'),
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
}