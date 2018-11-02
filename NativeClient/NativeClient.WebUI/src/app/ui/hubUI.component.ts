import { Component } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { pipe } from 'rxjs';
import { filter, map, merge, mergeMap } from 'rxjs/operators';

@Component({
    selector: 'app-hub-ui-main',
    templateUrl: './hubUI.component.html'
})
export class HubUIComponent {
    private _hostingForm = false;

    public get hostingForm() {
        return this._hostingForm;
    }

    constructor(
        private router: Router,
        private activatedRoute: ActivatedRoute
    ) {
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            map(_ => this.activatedRoute.firstChild),
            mergeMap(route => route.url)
        ).subscribe(event => {
            console.log('NavigationEnd:', event[0].path);
            // this._hostingForm = segments.findIndex(s => s === 'form') === -1;
        });
    }
}
