import { AbstractControl, ValidatorFn } from '@angular/forms';

export const passwordMatchValidator: ValidatorFn = (control: AbstractControl): { [key: string]: boolean } | null => {
    const password = control.get('password');
    const confirmPassword = control.get('confirmPassword');

    if (!password || !confirmPassword || password.pristine || confirmPassword.pristine) {
        return null;
    }

    if (password.value === confirmPassword.value) {
        return null;
    }

    return { 'passwordMismatch': true };
};