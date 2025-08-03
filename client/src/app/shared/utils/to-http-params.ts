import { HttpParams } from '@angular/common/http';

export const toHttpParams = (obj: any): HttpParams => {
    let params = new HttpParams();

    if (!obj) return params;

    Object.keys(obj).forEach(key => {
        const value = obj[key];
        if (value !== null && value !== undefined && value !== '') {
            params = params.set(key, value.toString());
        }
    });

    return params;
}
