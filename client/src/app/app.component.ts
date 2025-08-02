import { HttpClient } from '@angular/common/http';
import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'frontend';

  private readonly authService: AuthService = inject(AuthService);

  constructor(private readonly http: HttpClient) { }

  ngOnInit(): void {
    this.http.get('/api/tasks').subscribe(response => {
      console.log(response);
    });
  }
}
