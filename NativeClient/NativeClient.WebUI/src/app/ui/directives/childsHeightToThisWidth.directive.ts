import { Directive, ContentChild, QueryList, ElementRef, HostListener } from "@angular/core";

import { SizeSetterDirective } from "./sizeSetter.directive";

@Directive({
    selector: ".nmChildsHeightToThisWidth"
})
export class ChildsHeightToThisWidthDirective {
    @ContentChild(SizeSetterDirective)
    sizeSetter:SizeSetterDirective;
    //sizeSetters: QueryList<SizeSetterDirective>;

    constructor(private elementRef: ElementRef) {}

    private updateChildSizes() {
        //this.sizeSetters.forEach(child => child.setHeight(
        //    this.elementRef.nativeElement.offsetWidth 
        //));
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