import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ModelModule } from '../model/model.module';

import { ChildsHeightToThisWidthDirective } from './directives/childsHeightToThisWidth.directive';
import { SizeSetterDirective } from './directives/sizeSetter.directive';

import { PingDisplayComponent } from './misc/pingDisplay.component';
import { WebServicesButtonsComponent } from './misc/webServicesButtons.component';
import { ExecServicesButtonsComponent } from './misc/execServicesButtons.component';
import { LoadingIndicatorComponent } from './misc/loadingIndicator.component';
import { NodeInfoPopupComponent } from './misc/nodeInfoPopup.component';
import { BackendErrorSelectorComponent } from './messeging/backendErrorSelector.component';
import { MessageDisplayComponent } from './messeging/messageDisplay.component';
import { TaggedNodeListComponent } from './taggedNodeList.component';
import { NodesTreeViewComponent } from './nodesTreeView.component';
import { NumValueSwitchComponent } from './misc/numValueSwitch.component';
import { NumRangeSelectorComponent } from './misc/numRangeSelector.component';
import { SettingsProfileSelectionComponent } from './crud/settingsProfileSelection.component';
import { SettingsProfileFormComponent } from './crud/settingsProfileForm.component';
import { UIMainComponent } from './uiMain.component';
import { UIRoutingConfig } from './ui.routing';
import { TagSelectionComponent } from './crud/tagSelection.component';
import { ConfirmationPopupComponent } from './misc/confirmationPopup.component';
import { MessageTextSelectorComponent } from './messeging/messageTextSelector.component';
import { TagFormComponent } from './crud/tagForm.component';
import { CrudSelectorTemplateComponent } from './crud/crudSelectorTemplate.component';
import { CrudFormScaffoldingComponent } from './crud/crudFormScaffolding.component';
import { CrudFormHeaderComponent } from './crud/crudFormHeader.component';
import { CustomWebServiceSelectionComponent } from './crud/customWebServiceSelection.component';
import { CustomWebServiceFormComponent } from './crud/customWebServiceForm.component';
import { NtwkNodeSelectionComponent } from './crud/ntwkNodeSelection.component';
import { NtwkNodeFormComponent } from './crud/ntwkNodeForm.component';
import { IPValidatorDirective } from './directives/ipvalidator.directive';
import { TagsBindingSideSelectorComponent } from './crud/tagsBindingSideSelector.component';
import { TagsBindingComponent } from './crud/tagsBinding.component';
import { CrudNodeSideSelectorTemplateComponent } from './crud/crudNodeSideSelectorTemplate.component';
import { CustomWebServiceBindingSideSelectorComponent } from './crud/customWebServiceBindingSideSelector.component';
import { CustomWebServiceBindingComponent } from './crud/customWebServiceBinding.component';
import { HubUIComponent } from './hubUI.component';
import { EditorComponent } from './crud/editor.component';
import { FormHostComponent } from './crud/formHost.component';
import { TagFilterComponent } from './tagFilter.component';
import { PingMonitorPanelComponent } from './pingMonitorPanel.component';
import { MonitorSessionViewerComponent } from './misc/monitorSessionViewer.component';
import { PopupPanelContainerComponent } from './misc/popupPanelContainer.component';
import { MonitorMutabilityBlockComponent } from './misc/monitorMutabilityBlock.component';

@NgModule({
  declarations: [
    UIMainComponent,
    SettingsProfileSelectionComponent,
    ChildsHeightToThisWidthDirective,
    SizeSetterDirective,
    IPValidatorDirective,
    PingDisplayComponent,
    NumValueSwitchComponent,
    NumRangeSelectorComponent,
    WebServicesButtonsComponent,
    ExecServicesButtonsComponent,
    LoadingIndicatorComponent,
    NodeInfoPopupComponent,
    MessageTextSelectorComponent,
    BackendErrorSelectorComponent,
    MessageDisplayComponent,
    TaggedNodeListComponent,
    NodesTreeViewComponent,
    SettingsProfileFormComponent,
    TagFormComponent,
    TagSelectionComponent,
    ConfirmationPopupComponent,
    CrudSelectorTemplateComponent,
    CrudFormScaffoldingComponent,
    CrudFormHeaderComponent,
    CustomWebServiceSelectionComponent,
    CustomWebServiceFormComponent,
    NtwkNodeSelectionComponent,
    NtwkNodeFormComponent,
    TagsBindingSideSelectorComponent,
    TagsBindingComponent,
    CrudNodeSideSelectorTemplateComponent,
    CustomWebServiceBindingSideSelectorComponent,
    CustomWebServiceBindingComponent,
    HubUIComponent,
    EditorComponent,
    FormHostComponent,
    TagFilterComponent,
    PingMonitorPanelComponent,
    MonitorSessionViewerComponent,
    PopupPanelContainerComponent,
    MonitorMutabilityBlockComponent
  ],
  imports: [
    BrowserAnimationsModule,
    FormsModule,
    RouterModule,
    CommonModule,
    ModelModule,
    RouterModule,
    RouterModule.forRoot(UIRoutingConfig.routes)
  ],
  exports: [
    UIMainComponent
  ],
  providers: [],
})
export class UIModule { }
