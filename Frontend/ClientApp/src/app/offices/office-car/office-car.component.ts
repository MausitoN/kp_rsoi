import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { CarInfo, CarShortInfo } from '../../cars/cars.component';
import { ApiService } from '../../services/api/api-service';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface OfficeCarInfo {
  officeUid: string;
  locationOffice: string;
  carUid: string;
  registrationNumber: string;
  availabilityScheduleFirst: string;
  availabilityScheduleSecond: string;
  brand: string;
  model: string;
  power: string;
  carType: string;
}

export interface OfficeInfo {
  id: string;
  location: string;
  availableCars: string;
}

export interface BookingInfo {
  CarUid: string;
  RegistrationNumber: string;
  TakenFromOffice: string;
  ReturnToOffice: string;
  BookingPeriod: string;
  UserUid: string;
}

@Component({
  selector: 'app-office-car',
  templateUrl: './office-car.component.html',
  styleUrls: ['./office-car.component.css']
})
export class OfficeCarComponent implements OnInit {
  officeCar: OfficeCarInfo;
  private officeId: string;
  private carId: string;

  private transformData = "";
  schedule = "";
  officeList = [];
  private locationOfficeId: string;
  private role = false;
  useruid = "";
  firstData = "";
  secondData = "";

  constructor(
    private apiService: ApiService,
    private activateRoute: ActivatedRoute,
    private router: Router,
    private notifier: NotifierService,
    private spinner: NgxSpinnerService,
    private authService: AuthService
  ) { }

  ngOnInit() {
    this.useruid = this.authService.getToken();
    this.role = this.authService.isLoggedIn();
    if (!this.role)
    {
      this.router.navigate(['/login']);
    }
    this.activateRoute.params.subscribe(params => this.officeId = params['idOffice']);
    this.activateRoute.params.subscribe(params => this.carId = params['idCar']);
    this.spinner.show();
    this.apiService.getOfficeCar(this.officeId, this.carId).subscribe((res: OfficeCarInfo) => {
      res.carType = this.getEnumCarType(res.carType);
      this.firstData = this.getTransformData(this.transformData, res.availabilityScheduleFirst);
      this.secondData = this.getTransformData(this.transformData, res.availabilityScheduleSecond);
      this.officeCar = res;
      this.spinner.hide();
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения информации об автомобиле: ' + err.statusText);
    });

    this.apiService.getOffices().subscribe((res: OfficeInfo[]) => {
      this.locationOfficeId = res[0].id;
      res.forEach((element: OfficeInfo) => {
        this.officeList.push({ id: element.id, location: element.location });
      });
      this.spinner.hide();
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения информации об офисе: ' + err.statusText);
    });
  }

  selectOffice(event: any): void {
    this.locationOfficeId = event.target.value;
  }

  createBooking() {
    if (this.schedule < this.officeCar.availabilityScheduleFirst || this.schedule > this.officeCar.availabilityScheduleSecond)
    {
      this.notifier.notify('error', 'Ошибка создания брони, данная дата недоступна для бронирования!');
    }
    else
    {
      let request: BookingInfo = {
        CarUid: this.officeCar.carUid,
        RegistrationNumber: String(this.officeCar.registrationNumber),
        TakenFromOffice: this.officeCar.officeUid,
        ReturnToOffice: this.locationOfficeId,
        BookingPeriod: this.schedule,
        UserUid: this.authService.getUserId()};

      this.apiService.createBooking(request).subscribe((res) => {
        this.notifier.notify('success', 'Заказ бронирования автомобиля успешно создан');
        this.router.navigate(['/booking']);
      }, (err) => {
        console.log(err);
        this.notifier.notify('error',
          'Ошибка создания брони: ' + err.statusText);
      });
      this.router.navigate(['/']);
    }
  }

  moveRoute() {
    this.router.navigate(['/']);
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

  getTransformData(transformData, data: string)
  {
    transformData = "";
    transformData = transformData + data[8] + data[9] + "/" + data[5] + data[6] + "/" + data[0] + data[1] + data[2] + data[3];
    return transformData;
  }
}
