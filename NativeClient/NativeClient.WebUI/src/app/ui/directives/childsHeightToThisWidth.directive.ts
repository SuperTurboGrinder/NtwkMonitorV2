import {
    Directive,
    ContentChild, ElementRef, HostListener,
    AfterContentInit } from '@angular/core';

import { SizeSetterDirective } from './sizeSetter.directive';

@Directive({
    // tslint:disable-next-line:directive-selector
    selector: '.nmChildsHeightToThisWidth'
})
export class ChildsHeightToThisWidthDirective implements AfterContentInit {
    @ContentChild(SizeSetterDirective)
    sizeSetter: SizeSetterDirective;
    // sizeSetters: QueryList<SizeSetterDirective>;

    constructor(private elementRef: ElementRef) {}

    private updateChildSizes() {
        // this.sizeSetters.forEach(child => child.setHeight(
        //    this.elementRef.nativeElement.offsetWidth
        // ));
        this.sizeSetter.setHeight(
            this.elementRef.nativeElement.offsetWidth
        );
    }

    ngAfterContentInit() {
        this.updateChildSizes();
    }

    @HostListener('window:resize', ['$event'])
    onResize(event) {
        this.updateChildSizes();
    }
}
