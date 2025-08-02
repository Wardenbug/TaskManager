import { inject, Input, signal, OnInit, OnDestroy, Directive } from '@angular/core';
import { ControlContainer, FormGroup, AbstractControl } from '@angular/forms';
import { Subscription } from 'rxjs';

@Directive()
export abstract class FormFieldBase implements OnInit, OnDestroy {
    @Input({ required: true }) controlKey = '';
    @Input() label = '';

    protected parentContainer = inject(ControlContainer);
    protected statusChangesSubscription: Subscription | undefined;

    errorMessage = signal('');

    get parentFormGroup(): FormGroup {
        return this.parentContainer.control as FormGroup;
    }

    get control(): AbstractControl | null {
        return this.parentFormGroup.get(this.controlKey);
    }

    ngOnInit(): void {
        if (this.control) {
            this.statusChangesSubscription = this.control.statusChanges?.subscribe(() => {
                this.updateErrorMessage();
            });
        }
    }

    ngOnDestroy(): void {
        this.statusChangesSubscription?.unsubscribe();
    }

    protected updateErrorMessage(): void {
        if (!this.control) {
            this.errorMessage.set('');
            return;
        }

        if (this.control.hasError('required')) {
            this.errorMessage.set('You must enter a value');
        } else {
            this.handleSpecificErrors();
        }
    }

    protected handleSpecificErrors(): void {
        const errors = this.control?.errors;
        if (errors) {
            const errorKeys = Object.keys(errors);
            this.errorMessage.set(`Invalid ${this.label}`);
            throw new Error(`Unhandled validation errors for ${this.controlKey}: ${errorKeys.join(', ')}`);
        } else {
            this.errorMessage.set('');
        }
    }
}