import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet,
    MatSlideToggleModule
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
