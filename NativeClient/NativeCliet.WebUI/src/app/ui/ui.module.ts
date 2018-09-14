import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ModelModule } from "../model/model.module";

import { ChildsHeightToThisWidthDirective } from "./directives/childsHeightToThisWidth.directive";
import { SizeSetterDirective } from "./directives/sizeSetter.directive";

import { PingDisplayComponent } from "./misc/pingDisplay.component";
import { WebServicesButtonsComponent } from "./misc/webServicesButtons.component";
import { ExecServicesButtonsComponent } from "./misc/execServicesButtons.component";
import { LoadingIndicatorComponent } from "./misc/loadingIndicator.component";
import { NodeInfoPopupComponent } from './misc/nodeInfoPopup.component';
import { BackendErrorSelectorComponent } from './messeging/backendErrorSelector.component';
import { MessageDisplayComponent } from './messeging/messageDisplay.component';
import { TaggedNodeListComponent } from './taggedNodeList.component';
import { NodesTreeViewComponent } from './nodesTreeView.component';

@NgModule({
  declarations: [
    ChildsHeightToThisWidthDirective,
    SizeSetterDirective,
    PingDisplayComponent,
    WebServicesButtonsComponent,
    ExecServicesButtonsComponent,
    LoadingIndicatorComponent,
    NodeInfoPopupComponent,
    BackendErrorSelectorComponent,
    MessageDisplayComponent,
    TaggedNodeListComponent,
    NodesTreeViewComponent
  ],
  imports: [
    BrowserAnimationsModule, CommonModule, ModelModule
  ],
  exports: [
    MessageDisplayComponent,
    NodeInfoPopupComponent,
    TaggedNodeListComponent,
    NodesTreeViewComponent
  ],
  providers: [],
})
export class UIModule { }