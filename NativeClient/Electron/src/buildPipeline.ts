import { normalize } from 'path';
import { spawn } from 'child_process';
import * as fs from 'fs-extra';

class BuildPipeline {
    private componentsBuildPath = __dirname + '/../';
    private webAPIAppDir = 'NativeClient.WebAPI';
    private webUIAppDir = 'NativeClient.WebUI';

    private runConsoleCommand(cwd: string, cmd: string, args: string[]): Promise<number> {
        const execution = spawn(cmd, args, { cwd });
        execution.stdout.pipe(process.stdout);
        execution.stderr.pipe(process.stderr);
        return new Promise<number>((resolve, reject) => {
            execution.on('close', (code) => resolve(code));
        });
    }

    private isFileOK(path: string): Promise<boolean> {
        return new Promise<boolean>((resolve, reject) => {
            fs.stat(normalize(path), (err, stats) => {
                resolve(err == null);
            });
        });
    }

    private async isElectronAppBuilt(): Promise<boolean> {
        const electronBase = __dirname+'/app/js/';
        return await this.isFileOK(electronBase+'main.js')
        && await this.isFileOK(electronBase+'renderer.js');
    }

    private async isUIAppBuilt(): Promise<boolean> {
        const uiDistBase = this.componentsBuildPath+this.webUIAppDir+'dist';
        return await this.isFileOK(uiDistBase+'WebUI');
    }

    private async isAPIAppBuilt(platform: string, arch: string): Promise<boolean> {
        const dotnetRuntimeID =
            this.packagerPlatformAndArchToDotnetRuntimeID(platform, arch);
        if (dotnetRuntimeID !== null) {
            const apiReleaseBinBase = this.componentsBuildPath+this.webAPIAppDir
            +'/bin/release/netcoreapp2.2/';
            return await this.isFileOK(apiReleaseBinBase+dotnetRuntimeID);
        } else {
            console.log(`Wrong platform (${platform}) or arch (${arch}).`);
            return false;
        }
    }

    private packagerPlatformAndArchToDotnetRuntimeID(
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
            return null;
        } else {
            return dotnetPlatform+'-'+dotnetArch;
        }
    }

    private buildElectronApp(): Promise<number> {
        console.log('Building ElectronApp');
        return this.runConsoleCommand(__dirname, 'npm', ['run', 'build']);
    }

    private async buildWebAPI(platform: string, arch: string): Promise<number> {
        console.log('Building WebAPI');
        const dotnetRuntimeID =
            this.packagerPlatformAndArchToDotnetRuntimeID(platform, arch);
        if (dotnetRuntimeID !== null) {
            const apiProjectDir = this.componentsBuildPath + this.webAPIAppDir;
            return this.runConsoleCommand(
                apiProjectDir,
                'dotnet',
                ['publish', '-c', 'release', '-r', dotnetRuntimeID]
            );
        } else {
            console.log(`Wrong platform (${platform}) or arch (${arch}).`);
            return -1;
        }
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

    async rebuildAllSubprojects(platform: string, arch: string): Promise<boolean> {
        const electronBuildIsOK = (await this.buildElectronApp()) === 0;
        const apiBuildIsOK = electronBuildIsOK
        && (await this.buildWebAPI(platform, arch)) === 0;
        const uiBuildISOK = apiBuildIsOK && (await this.buildWebUI()) === 0;
        return uiBuildISOK;
    }
    

    async buildAllUnbuiltSubprojects(platform: string, arch: string): Promise<boolean> {
        const electron = await this.isElectronAppBuilt()
        && await this.buildElectronApp() === 0;
        if (electron === false) return false;
        const webUI = await this.isUIAppBuilt()
        && await this.buildWebUI() === 0;
        if (webUI === false) return false;
        return await this.isAPIAppBuilt(platform, arch)
        && await this.buildWebAPI(platform, arch) === 0;
    }

    async copyWebUITo(path: string): Promise<boolean> {

    }
    
}