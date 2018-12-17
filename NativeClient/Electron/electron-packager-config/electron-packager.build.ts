import * as packager from 'electron-packager';
import { BuildPipeline } from './buildPipeline';

let buildPipeline: BuildPipeline = null;
packager({
    dir: 'app',
    executableName: 'NetworkMonitorV2',
    name: 'NetMonitorV2',
    // icon: 'icon.png',
    asar: true,
    afterExtract: [(buildPath: string, electronVersion: string,
    platform: string, arch: string, callback: ()=>void) => {
        buildPipeline = new BuildPipeline(platform, arch);
        buildPipeline.buildAllUnbuiltSubprojects().then(built => {
            if (built) {
                buildPipeline.copyWebAPITo(buildPath).then(copyed =>
                    callback()
                );
            }
        });
    }],
    // afterCopy: [(buildPath, electronVersion, platform, arch, callback) => {}],
    afterPrune: [(buildPath: string, electronVersion: string,
    platform: string, arch: string, callback: ()=>void) => {
        buildPipeline.copyWebUITo(buildPath).then(() => callback());
    }]
})
.then((appPath: string | string[]) => {
    console.log(`Electron app was packaged to ${appPath}`);
});