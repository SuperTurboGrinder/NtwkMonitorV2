import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { UIModule } from './ui/ui.module';
import { ServicesModule } from './services/services.module';

import { AppComponent } from './app.component';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule, UIModule, ServicesModule],
  providers: [
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
