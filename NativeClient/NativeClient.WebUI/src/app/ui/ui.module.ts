import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
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
import { NumValueSwitchComponent } from './misc/numValueSwitch.component';
import { NumRangeSelectorComponent } from './misc/numRangeSelector.component';
import { SettingsProfileSelectionComponent } from './settingsProfileSelection.component';
import { SettingsProfileFormComponent } from './crud/settingsProfileForm.component';

@NgModule({
  declarations: [
    SettingsProfileSelectionComponent,
    ChildsHeightToThisWidthDirective,
    SizeSetterDirective,
    PingDisplayComponent,
    NumValueSwitchComponent,
    NumRangeSelectorComponent,
    WebServicesButtonsComponent,
    ExecServicesButtonsComponent,
    LoadingIndicatorComponent,
    NodeInfoPopupComponent,
    BackendErrorSelectorComponent,
    MessageDisplayComponent,
    TaggedNodeListComponent,
    NodesTreeViewComponent,
    SettingsProfileFormComponent
  ],
  imports: [
    BrowserAnimationsModule, FormsModule, RouterModule, CommonModule, ModelModule
  ],
  exports: [
    MessageDisplayComponent,
    NodeInfoPopupComponent,
    TaggedNodeListComponent,
    SettingsProfileSelectionComponent,
    NodesTreeViewComponent,
    SettingsProfileFormComponent
  ],
  providers: [],
})
export class UIModule { }