import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { from, Observable } from 'rxjs';
import { ApiService } from '../services/api/api-service';
import { NgxSpinnerService } from "ngx-spinner";
import { AuthService } from '../services/auth-service';

export interface AllUsersInfo {
  uid: string;
  login: string;
  password: string;
  role: string;
}

@Component({
  selector: 'app-users',
  styleUrls: ['./users.component.css'],
  templateUrl: './users.component.html',
})

export class UsersComponent implements OnInit {
  private users: AllUsersInfo[] = [];
  notnullUser = false;
  nullUser = false;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private spinnerService: NgxSpinnerService,
    private authService: AuthService,
    private notifier: NotifierService
  ) {
  }
  ngOnInit(): void {
    if (this.authService.getUserRole() != 'admin')
    {
      this.router.navigate(['/login']);
    }
    this.spinnerService.show();
    this.apiService.getAllUsers().subscribe((res: object[]) => {
      let newUsers = [];
      res.forEach((element: AllUsersInfo) => {
        if (element.role == null)
        {
          element.role = "Пользователь";
        }
        if (element.role == "admin")
        {
          element.role = "Админ";
        }
        newUsers.push(element);
        this.notnullUser = true;
      });
      this.users = newUsers;
      this.spinnerService.hide();
      if (!this.notnullUser)
      {
          this.nullUser = true;
      }
    }, (err) => {
      this.spinnerService.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка пользователей: ' + err.statusText);
    });
  }
}
