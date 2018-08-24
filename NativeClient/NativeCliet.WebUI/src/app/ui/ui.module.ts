import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { ModelModule } from "../model/model.module";
import { BackendErrorSelectorComponent } from './messeging/backendErrorSelector.component';
import { MessageDisplayComponent } from './messeging/messageDisplay.component';
import { TaggedNodeListComponent } from './taggedNodeList.component';

@NgModule({
  declarations: [
    BackendErrorSelectorComponent,
    MessageDisplayComponent,
    TaggedNodeListComponent
  ],
  imports: [
    BrowserAnimationsModule, CommonModule, ModelModule
  ],
  exports: [
    MessageDisplayComponent,
    TaggedNodeListComponent
  ],
  providers: [],
})
export class UIModule { }