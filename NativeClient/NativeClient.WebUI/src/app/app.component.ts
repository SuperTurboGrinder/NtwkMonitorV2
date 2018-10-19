import { Component } from '@angular/core';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'Network Monitor V2';
  testInterval: Observable<number>;

  constructor(
  ) { }
}
