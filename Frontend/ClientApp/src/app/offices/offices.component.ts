import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../services/api/api-service';
import { AuthService } from '../services/auth-service';

export interface OfficeInfo {
  id: string;
  location: string;
  availableCars: string;
}

@Component({
  selector: 'app-offices',
  templateUrl: './offices.component.html',
  styleUrls: ['./offices.component.css']
})

export class OfficesComponent implements OnInit {
  private offices: OfficeInfo[] = [];

  notnullOffice = false;
  nullOffice = false;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private spinner: NgxSpinnerService,
    private notifier: NotifierService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.spinner.show();
    this.apiService.getOffices().subscribe((res: object[]) => {
      let newOffices = [];
      res.forEach((element: OfficeInfo) => {
        newOffices.push(element);
        this.notnullOffice = true;
      });
      this.offices = newOffices;
      this.spinner.hide();
      if (!this.notnullOffice)
      {
          this.nullOffice = true;
      }
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка офисов: ' + err.statusText);
    });
  }

  gotoOfficeCars(officeId: string) {
    this.router.navigate(['office', officeId, 'cars']);
  }

}
