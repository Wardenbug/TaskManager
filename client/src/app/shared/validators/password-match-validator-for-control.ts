import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export function passwordMatchValidatorForControl(passwordControl: AbstractControl | null): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
        if (!passwordControl || !control) {
            return null;
        }
        const passwordValue = passwordControl.value;
        const confirmPasswordValue = control.value;

        return passwordValue === confirmPasswordValue ? null : { passwordMismatch: true };
    };
}