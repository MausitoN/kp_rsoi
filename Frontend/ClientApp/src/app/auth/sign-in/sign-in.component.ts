import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api/api-service';
import { AuthService } from '../../services/auth-service';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-sign-in',
  templateUrl: './sign-in.component.html',
  styleUrls: ['./sign-in.component.css']
})
export class SignInComponent implements OnInit {

  username: string;
  password: string;

  constructor(
    private router: Router,
    private apiService: ApiService,
    private authService: AuthService,
    private notifierService: NotifierService,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit() {
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['']);
    }
  }

  login() {
    this.spinner.show();
    this.apiService.login(this.username, this.password).subscribe((res) => {
      this.authService.setSession(res);
      this.spinner.hide();
      this.notifierService.notify('success', 'Добро пожаловать, ' + this.username + '!');
      this.router.navigate(['']);
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifierService.notify('error', 'Ошибка авторизации');
      const errMsg: string = err.error.message === undefined ? err.statusText : err.error.message;
      this.notifierService.notify('error', 'Ошибка авториации: ' + errMsg);
    });
  }
}
