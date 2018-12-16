import { normalize } from 'path';
import { spawn } from 'child_process';
import * as fs from 'fs';

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

    private isFileOK(dirpath: string): Promise<boolean> {
        return new Promise<boolean>((resolve, reject) => {
            fs.stat(dirpath, (err, stats) => {
                resolve(err == null);
            });
        });
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
        return this.runConsoleCommand(__dirname, 'npm', ['run', 'build']);
    }

    private buildWebAPI(runtimeID: string): Promise<number> {
        const apiProjectDir = this.componentsBuildPath + this.webAPIAppDir;
        return this.runConsoleCommand(
            apiProjectDir,
            'dotnet',
            ['publish', '-c', 'release', '-r', runtimeID]
        );
    }

    private buildWebUI(): Promise<number> {
        const uiProjectDir = this.componentsBuildPath + this.webUIAppDir;
        return this.runConsoleCommand(
            uiProjectDir,
            'ng',
            ['build', '--prod', '--aot']
        );
    }

    async rebuildAllSubprojects(): Promise<boolean> {
        const electronBuildIsOK = (await this.buildElectronApp()) === 0;
        const apiBuildIsOK = electronBuildIsOK && (await this.buildWebAPI('')) === 0;
        const uiBuildISOK = apiBuildIsOK && (await this.buildWebUI()) === 0;
        return uiBuildISOK;
    }
    
}