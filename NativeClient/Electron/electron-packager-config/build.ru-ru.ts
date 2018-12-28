import { ElectronPackagerScript, PipelineType } from './electron-packager.common';

const packagerScript =
    new ElectronPackagerScript('ru-ru', PipelineType.BuildAndPack);
packagerScript.packageElectronApp();
