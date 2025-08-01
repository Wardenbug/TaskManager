import { inject } from '@angular/core';
import { CanActivateFn, RedirectCommand, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { firstValueFrom } from 'rxjs';

export const authGuard: CanActivateFn = async (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

    if (authService.User) return true;

   try {
    const user = await firstValueFrom(authService.getUserInfo());
    authService.User = user;
    return true;
  } catch {
    return new RedirectCommand(router.parseUrl('auth/login'));
  }

  return true;
};
