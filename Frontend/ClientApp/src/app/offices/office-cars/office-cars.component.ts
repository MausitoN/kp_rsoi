import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { CarInfo, CarShortInfo } from '../../cars/cars.component';
import { ApiService } from '../../services/api/api-service';
import { OfficeInfo } from '../offices.component';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface OfficeCarsInfo {
  officeUid:string;
  locationOffice: string;
  carUid: string;
  registrationNumber: string;
  availabilitySchedule: string;
  brand: string;
  model: string;
  power: string;
  carType: string;
}

@Component({
  selector: 'app-office-cars',
  templateUrl: './office-cars.component.html',
  styleUrls: ['./office-cars.component.css']
})
export class OfficeCarsComponent implements OnInit {
  private officeCars: OfficeCarsInfo[] = [];
  private location: string;
  private officeId: string;
  private office: OfficeInfo;

  notnullCar = false;
  nullCar = false;
  role = "";

  constructor(
    private apiService: ApiService,
    private activateRoute: ActivatedRoute,
    private router: Router,
    private notifier: NotifierService,
    private spinner: NgxSpinnerService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.role = this.authService.getUserRole();
    this.activateRoute.params.subscribe(params => this.officeId = params['id']);
    this.spinner.show();
    this.apiService.getOfficeCars(this.officeId).subscribe((res: OfficeCarsInfo[]) => {
      res.forEach((element: OfficeCarsInfo) => {
        this.location = element.locationOffice;
        element.carType = this.getEnumCarType(element.carType);
        this.officeCars.push(element);
        this.notnullCar = true;
      });
      this.spinner.hide();
      if (!this.notnullCar)
      {
          this.nullCar = true;
      }
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка автомобилей в офисе: ' + err.statusText);
    });
  }

  refresh() {
    this.spinner.show();
    let newOfficeCars = [];
    this.apiService.getOfficeCars(this.officeId).subscribe((res: OfficeCarsInfo[]) => {
      res.forEach((element: OfficeCarsInfo) => {
        element.carType = this.getEnumCarType(element.carType);
        newOfficeCars.push(element);
      });
      this.officeCars = newOfficeCars;
      this.spinner.hide();
      if (!this.notnullCar)
      {
          this.nullCar = true;
      }
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка автомобилей в офисе: ' + err.statusText);
    });
  }

  gotoOfficeCar(carId: string, officeId: string) {
    this.router.navigate(['office', officeId, 'car', carId]);
  }

  gotoAddOfficeCars(officeId: string) {
    this.router.navigate(['office/add-office-cars', officeId]);
  }

  deleteCarFromOffice(carId: string, officeId: string) {
    this.apiService.deleteCarFromOffice(carId, officeId).subscribe((res) => {
      this.notifier.notify('success', 'Автомобиль успешно удален');
      this.refresh();
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        'Ошибка удаления автомобиля из офиса: ' + err.statusText);
    });
    this.refresh();
  }

  getEnumCarType(value: string) {
    if (value == "SEDAN")
    {
      value = "Седан"
    }
    else if (value == "SUV")
    {
      value = "Внедорожник"
    }
    else if (value == "MINIVAN")
    {
      value = "Минивэн"
    }
    else if (value == "ROADSTER")
    {
      value = "Родстер"
    }
    return value;
  }
}
