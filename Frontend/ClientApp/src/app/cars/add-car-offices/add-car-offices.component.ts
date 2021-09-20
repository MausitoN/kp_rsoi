import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface OfficeInfo {
  id: string;
  location: string;
}

@Component({
  selector: 'app-add-car-offices',
  templateUrl: './add-car-offices.component.html',
  styleUrls: ['./add-car-offices.component.css']
})
export class OfficeComponent implements OnInit {
  private offices: OfficeInfo[] = [];
  private carId: string;

  notnullOffice = false;
  nullOffice = false;

  constructor(
    private apiService: ApiService,
    private activateRoute: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private notifier: NotifierService,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit() {
    if (this.authService.getUserRole() != 'admin')
    {
      this.router.navigate(['/login']);
    }
    this.activateRoute.params.subscribe(params => this.carId = params['id']);
    this.spinner.show();
    this.apiService.getOffices().subscribe((res: OfficeInfo[]) => {
      res.forEach((element: OfficeInfo) => {
        this.offices.push(element);
        this.notnullOffice = true;
      });
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

  addCarToOffice(carId: string, officeId: string) {
    this.apiService.addCarToOffice(carId, officeId).subscribe((res) => {
      this.notifier.notify('success', 'Автомобиль успешно добавлен в офис');
      this.router.navigate(['/']);
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка пользователей: ' + err.statusText);
    });
    this.router.navigate(['/']);
  }

}
