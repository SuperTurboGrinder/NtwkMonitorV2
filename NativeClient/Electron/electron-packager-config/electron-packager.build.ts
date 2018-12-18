import { BuildPipeline } from './buildPipeline';
import { packageElectronApp } from './electron-packager.common';

packageElectronApp(
    (pipeline: BuildPipeline) => pipeline.buildAllUnbuiltSubprojects()
);
