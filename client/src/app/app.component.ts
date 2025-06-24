import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ContainerComponent } from './core/layout/container/container.component';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    ContainerComponent
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'frontend';

  constructor(private readonly http: HttpClient) { }

  ngOnInit(): void {
    this.http.get('/api/tasks').subscribe(response => {
      console.log(response);
    });
  }
}
