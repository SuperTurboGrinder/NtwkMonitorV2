import * as packager from 'electron-packager';
import { BuildPipeline } from './buildPipeline';
import { normalize } from 'path';

export enum PipelineType {
    BuildAndPack,
    RebuildAndPack
}

export class ElectronPackagerScript {
    private options: packager.Options = {
        dir: 'app',
        executableName: 'NetworkMonitorV2',
        name: 'NetMonitorV2',
        asar: true
    };

    constructor(
        private locale: string,
        private buildType: PipelineType
    ) {
        const platform = process.platform;
        if (platform === 'win32'
        || platform === 'darwin'
        || platform === 'linux') {
            this.setAppIcon(platform);
            this.setPipelineFunctions();
        } else {
            console.log(`Platform not supported (${platform}).`);
            this.options = null;
        }
    }

    packageElectronApp() {
        packager(this.options)
        .then((appPath: string | string[]) => {
            console.log(`Electron app was packaged to ${appPath}`);
        })
        .catch(err => {
            console.log('Unexpected exception occured while building electron app.');
        });
    }

    private setAppIcon(platform: string) {
        const basePath = __dirname+'/../../exec_icons/icon.';
        switch (platform) {
            case 'win32':
                this.options.icon = normalize(basePath + 'ico');
            break;
            case 'darwin':
                this.options.icon = normalize(basePath + 'icns');
            break;
            case 'linux':
                this.options.icon = normalize(basePath + 'png');
            break;
            default:
        }
    }

    private setPipelineFunctions() {
        let buildPipeline: BuildPipeline = null;
        this.options.afterExtract = [async (buildPath: string, electronVersion: string,
        platform: string, arch: string, callback: ()=>void) => {
            buildPipeline = new BuildPipeline(this.locale, platform, arch);
            let built = false;
            switch (this.buildType) {
                case PipelineType.BuildAndPack:
                    built = await buildPipeline.buildAllUnbuiltSubprojects();
                break;
                case PipelineType.RebuildAndPack:
                    built = await buildPipeline.rebuildAllSubprojects();
                break;
                default:
                    console.log('Unimplemented pipeline type.');
                    return;
            }
            if (built) {
                await buildPipeline.copyWebAPITo(buildPath);
                await buildPipeline.copyAPIDatabaseTo(buildPath);
                callback();
            } else {
                console.log('Failed to build app component projects.');
            }
        }];
        // afterCopy: [(buildPath, electronVersion, platform, arch, callback) => {}],
        this.options.afterPrune = [(buildPath: string, electronVersion: string,
        platform: string, arch: string, callback: ()=>void) => {
            buildPipeline.copyWebUITo(buildPath).then(() => callback());
        }];
    }
}
