!function(e){var t={};function i(n){if(t[n])return t[n].exports;var o=t[n]={i:n,l:!1,exports:{}};return e[n].call(o.exports,o,o.exports,i),o.l=!0,o.exports}i.m=e,i.c=t,i.d=function(e,t,n){i.o(e,t)||Object.defineProperty(e,t,{enumerable:!0,get:n})},i.r=function(e){"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(e,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(e,"__esModule",{value:!0})},i.t=function(e,t){if(1&t&&(e=i(e)),8&t)return e;if(4&t&&"object"==typeof e&&e&&e.__esModule)return e;var n=Object.create(null);if(i.r(n),Object.defineProperty(n,"default",{enumerable:!0,value:e}),2&t&&"string"!=typeof e)for(var o in e)i.d(n,o,function(t){return e[t]}.bind(null,o));return n},i.n=function(e){var t=e&&e.__esModule?function(){return e.default}:function(){return e};return i.d(t,"a",t),t},i.o=function(e,t){return Object.prototype.hasOwnProperty.call(e,t)},i.p="",i(i.s=7)}([function(e,t,i){"use strict";var n=this&&this.__awaiter||function(e,t,i,n){return new(i||(i=Promise))(function(o,r){function s(e){try{a(n.next(e))}catch(e){r(e)}}function l(e){try{a(n.throw(e))}catch(e){r(e)}}function a(e){e.done?o(e.value):new i(function(t){t(e.value)}).then(s,l)}a((n=n.apply(e,t||[])).next())})};Object.defineProperty(t,"__esModule",{value:!0});const o=i(1),r=i(2);t.packageElectronApp=function(e){let t=null;o({dir:"app",executableName:"NetworkMonitorV2",name:"NetMonitorV2",asar:!0,afterExtract:[(i,o,s,l,a)=>n(this,void 0,void 0,function*(){t=new r.BuildPipeline(s,l),(yield e(t))?(yield t.copyWebAPITo(i),yield t.copyAPIDatabaseTo(i),a()):console.log("Failed to build app component projects.")})],afterPrune:[(e,i,n,o,r)=>{t.copyWebUITo(e).then(()=>r())}]}).then(e=>{console.log(`Electron app was packaged to ${e}`)}).catch(e=>{console.log("Unexpected exception occured while building electron app.")})}},function(e,t){e.exports=require("electron-packager")},function(e,t,i){"use strict";var n=this&&this.__awaiter||function(e,t,i,n){return new(i||(i=Promise))(function(o,r){function s(e){try{a(n.next(e))}catch(e){r(e)}}function l(e){try{a(n.throw(e))}catch(e){r(e)}}function a(e){e.done?o(e.value):new i(function(t){t(e.value)}).then(s,l)}a((n=n.apply(e,t||[])).next())})};Object.defineProperty(t,"__esModule",{value:!0});const o=i(3),r=i(4),s=i(5);t.BuildPipeline=class{constructor(e,t){this.DEBUG_BUILD_PIPELINE=!1,this.electronProjectPath=o.normalize(__dirname+"/../../"),this.componentsBuildPath=o.normalize(this.electronProjectPath+"../"),this.npmCommand="win32"===process.platform?"npm.cmd":"npm",this.emptyDatabaseFilename="__empty__.sqlite",this.webAPIAppDir="NativeClient.WebAPI",this.webUIAppDir="NativeClient.WebUI",this.dotnetRuntimeID=null,this.dotnetRuntimeID=this._packagerPlatformAndArchToDotnetRuntimeID(e,t)}_packagerPlatformAndArchToDotnetRuntimeID(e,t){const i=new Map([["win32","win"],["linux","linux"],["darvin","osx"]]).get(e),n=new Map([["ia32","x86"],["x64","x64"]]).get(t);if(void 0===n||void 0===i||"x86"===n&&"win"!==i)throw new Error(`Wrong platform (${e}) or arch (${t}).`);return i+"-"+n}runConsoleCommand(e,t,i){const n=o.normalize(e);if(!0===this.DEBUG_BUILD_PIPELINE){const e=i.reduce((e,t)=>`${e} ${t}`,t);console.log(`Running '${e}' in ${n} folder`)}const s=r.spawn(t,i,{cwd:n});return s.stdout.pipe(process.stdout),s.stderr.pipe(process.stderr),new Promise((e,t)=>{s.on("close",t=>e(t))})}isPathOK(e){return n(this,void 0,void 0,function*(){const t=o.normalize(e),i=yield s.pathExists(t);return!0===this.DEBUG_BUILD_PIPELINE&&(!0===i?console.log(`Path exists: ${t}`):console.log(`Path does not exist: ${t}`)),i})}isElectronAppBuilt(){return n(this,void 0,void 0,function*(){const e=__dirname+"/../../app/js/",t=(yield this.isPathOK(e+"main.js"))&&(yield this.isPathOK(e+"renderer.js"));return!0===t?console.log("Found built base Electron app project."):console.log("Base electron app project is not built."),t})}isUIAppBuilt(){return n(this,void 0,void 0,function*(){const e=this.componentsBuildPath+this.webUIAppDir+"/dist/",t=yield this.isPathOK(e+"WebUI");return!0===t?console.log("Found built UI project."):console.log("UI project is not built."),t})}isAPIAppBuilt(){return n(this,void 0,void 0,function*(){const e=this.componentsBuildPath+this.webAPIAppDir+"/bin/release/netcoreapp2.2/",t=yield this.isPathOK(e+this.dotnetRuntimeID+"/publish");return!0===t?console.log("Found built API project."):console.log("API project is not built."),t})}buildElectronApp(){return console.log("Building base Electron app."),this.runConsoleCommand(__dirname+"/../../",this.npmCommand,["run","build"])}buildWebUI(){console.log("Building WebUI."),console.log("This may take some time...");const e=this.componentsBuildPath+this.webUIAppDir;return this.runConsoleCommand(e,this.npmCommand,["run","build"])}buildWebAPI(){return n(this,void 0,void 0,function*(){console.log("Building WebAPI.");const e=this.componentsBuildPath+this.webAPIAppDir;return this.runConsoleCommand(e,"dotnet",["publish","-c","release","-r",this.dotnetRuntimeID])})}buildEmptyAPIDatabase(e=!1){return n(this,void 0,void 0,function*(){const t=this.componentsBuildPath+this.webAPIAppDir,i=o.normalize(t+"/"+this.emptyDatabaseFilename);if(!0===(yield this.isPathOK(i))){if(!0!==e)return console.log("Emtpy database already created."),0;yield s.remove(i)}console.log("Building empty database.");const n=o.normalize(t+"/DB.sqlite"),r=o.normalize(t+"/__temp_renamed__.sqlite"),l=this.isPathOK(n);l&&(yield s.rename(n,r));const a=yield this.runConsoleCommand(t,"dotnet",["ef","database","update"]);return 0===a&&(yield s.rename(n,i)),l&&(yield s.rename(r,n)),a})}rebuildAllSubprojects(){return n(this,void 0,void 0,function*(){return 0===(yield this.buildElectronApp())&&0===(yield this.buildWebAPI())&&!(0!==(yield this.buildWebUI()))&&0===(yield this.buildEmptyAPIDatabase(!0))})}buildAllUnbuiltSubprojects(){return n(this,void 0,void 0,function*(){return!1!==((yield this.isElectronAppBuilt())||0===(yield this.buildElectronApp()))&&!1!==((yield this.isUIAppBuilt())||0===(yield this.buildWebUI()))&&!1!==((yield this.isAPIAppBuilt())||0===(yield this.buildWebAPI()))&&0===(yield this.buildEmptyAPIDatabase())})}copyWebUITo(e){return n(this,void 0,void 0,function*(){console.log("Copying WebUI dir to electron package..."),yield s.copy(o.normalize(this.componentsBuildPath+this.webUIAppDir+"/dist/WebUI"),o.normalize(e+"/WebUI")),console.log("Finished copying WebUI dir.")})}copyWebAPITo(e){return n(this,void 0,void 0,function*(){console.log("Copying WebAPI dir to electron package...");const t=this.componentsBuildPath+this.webAPIAppDir+"/bin/release/netcoreapp2.2/"+this.dotnetRuntimeID+"/publish";yield s.copy(o.normalize(t),o.normalize(e+"/WebAPI")),console.log("Finished copying WebAPI dir.")})}copyAPIDatabaseTo(e){return n(this,void 0,void 0,function*(){console.log("Copying empty database to electron package...");const t=o.normalize(this.componentsBuildPath+this.webAPIAppDir+"/"+this.emptyDatabaseFilename),i=o.normalize(e+"/WebAPI/DB.sqlite");yield s.copy(t,i),console.log("Finished copying empty database.")})}}},function(e,t){e.exports=require("path")},function(e,t){e.exports=require("child_process")},function(e,t){e.exports=require("fs-extra")},,function(e,t,i){"use strict";Object.defineProperty(t,"__esModule",{value:!0}),i(0).packageElectronApp(e=>e.rebuildAllSubprojects())}]);