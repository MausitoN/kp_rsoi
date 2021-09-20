import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface CarOfficesInfo {
  officeUid: string;
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
  selector: 'app-car-offices',
  templateUrl: './car-offices.component.html',
  styleUrls: ['./car-offices.component.css']
})
export class CarOfficesComponent implements OnInit {
  private carOffices: CarOfficesInfo[] = [];
  private carId: string;

  private brand: string;
  private model: string;
  private power: string;
  private carType: string;

  notnullOffice = false;
  nullOffice = false;
  role = "";

  constructor(
    private apiService: ApiService,
    private activateRoute: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private notifier: NotifierService,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit() {
    this.role = this.authService.getUserRole();
    this.activateRoute.params.subscribe(params => this.carId = params['id']);
    this.spinner.show();
    this.apiService.getCarOffices(this.carId).subscribe((res: CarOfficesInfo[]) => {
      res.forEach((element: CarOfficesInfo) => {
        this.brand = element.brand;
        this.model = element.model;
        this.power = element.power;
        this.carType = this.getEnumCarType(element.carType);
        this.carOffices.push(element);
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
        'Ошибка получения списка офисов, в которых есть этот автомобиль: ' + err.statusText);
    });
  }

  refresh() {
    this.spinner.show();
    let newCarOffices = [];
    this.apiService.getCarOffices(this.carId).subscribe((res: CarOfficesInfo[]) => {
      res.forEach((element: CarOfficesInfo) => {
        newCarOffices.push(element);
      });
      this.carOffices = newCarOffices;
      this.spinner.hide();
      if (!this.notnullOffice)
      {
          this.nullOffice = true;
      }
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка офисов, в которых есть этот автомобиль: ' + err.statusText);
    });
  }

  gotoOfficeCar(carId: string, officeId: string) {
    this.router.navigate(['office', officeId, 'car', carId]);
  }

  gotoAddCarOffices(carId: string) {
    this.router.navigate(['office/add-car-offices', carId]);
  }

  deleteCarFromOffice(carId: string, officeId: string) {
    this.apiService.deleteCarFromOffice(carId, officeId).subscribe((res) => {
      this.notifier.notify('success', 'Автомобиль успешно удален');
      this.refresh();
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка пользователей: ' + err.statusText);
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
