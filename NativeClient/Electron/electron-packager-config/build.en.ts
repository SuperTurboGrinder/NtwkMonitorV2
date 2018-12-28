import { ElectronPackagerScript, PipelineType } from './electron-packager.common';

const packagerScript =
    new ElectronPackagerScript('en', PipelineType.BuildAndPack);
packagerScript.packageElectronApp();
