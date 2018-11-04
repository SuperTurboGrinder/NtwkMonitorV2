import { Component, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router, NavigationEnd } from '@angular/router';
import { pipe, Subscription } from 'rxjs';
import { filter, map, merge, mergeMap } from 'rxjs/operators';

@Component({
    selector: 'app-hub-ui-main',
    templateUrl: './hubUI.component.html'
})
export class HubUIComponent implements OnDestroy {
    private _hostingForms = true;
    private readonly routerSubscription: Subscription;

    public get hostingForms() {
        return this._hostingForms;
    }

    constructor(
        router: Router,
        private activatedRoute: ActivatedRoute
    ) {
        this. routerSubscription = router.events.pipe(
            filter(event => event instanceof NavigationEnd),
            map(_ => this.activatedRoute.firstChild),
            filter(child => child !== null),
            mergeMap(route => route.url)
        ).subscribe(event => {
            if (event[0].path !== 'profilesSelect') {
                this._hostingForms = event[0].path === 'form';
            }
        });
    }

    public ngOnDestroy() {
        this.routerSubscription.unsubscribe();
    }
}
