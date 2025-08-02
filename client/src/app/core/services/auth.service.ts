import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { User } from '../models/user/user';
import { UserLogin } from '../models/user/user-login';
import { UserRegister } from '../models/user/user-register';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  public User: User | null = null;
  private readonly http: HttpClient = inject(HttpClient);

  public isAuthenticated(): boolean {
    return this.User !== null;
  }

  public login(userLogin: UserLogin) {
    return this.http.post<User>('api/users/login', userLogin);
  }

  public register(userRegister: UserRegister) {
    return this.http.post<User>('api/users/register', userRegister);
  }

  public getUserInfo() {
    return this.http.get<User>('/api/users/me', { withCredentials: true });
  }

  constructor() { }
}
