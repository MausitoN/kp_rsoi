import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface NewUserInfo {
  Login: string;
  Password: string;
}

@Component({
  selector: 'app-add-user',
  templateUrl: './add-user.component.html',
  styleUrls: ['./add-user.component.css']
})

export class AddUserComponent implements OnInit {
  login = "";
  password = "";

  constructor(
    private apiService: ApiService,
    private activateRoute: ActivatedRoute,
    private router: Router,
    private notifier: NotifierService,
    private spinner: NgxSpinnerService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    if (this.authService.getUserRole() != 'admin')
    {
      this.router.navigate(['/login']);
    }
  }

  addUser() {
    let request: NewUserInfo = {
      Login: this.login,
      Password: this.password };
    this.apiService.addNewUser(request).subscribe(() => {
      this.notifier.notify('success', 'Пользователь успешно удален');
      this.router.navigate(['/users']);
    },
    (err) => {
      this.router.navigate(['/users']);
      console.log(err);
      this.notifier.notify('error',
        'Ошибка добавления пользователя: ' + err.statusText);
    });
    
  }
}
