import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { from, Observable } from 'rxjs';
import { ApiService } from '../services/api/api-service';
import { NgxSpinnerService } from "ngx-spinner";
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../services/auth-service';

export interface CarShortInfo {
  uid: string;
  brand: string;
  model: string;
}

export interface CarInfo {
  id: string;
  brand: string;
  model: string;
  power: string;
  carType: string;
}

@Component({
  selector: 'app-cars',
  styleUrls: ['./cars.component.css'],
  templateUrl: './cars.component.html',
})

export class CarsComponent implements OnInit {
  name = '';
  author = '';
  private cars: CarInfo[] = [];

  role = '';
  notnullCar = false;
  nullCar = false;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private activateRoute: ActivatedRoute,
    private spinnerService: NgxSpinnerService,
    private notifier: NotifierService,
    private authService: AuthService,
  ) {
    
  }
  ngOnInit(): void {
    this.role = this.authService.getUserRole();
    this.spinnerService.show();
    this.apiService.getCars().subscribe((res: object[]) => {
      let newCars = [];
      res.forEach((element: CarInfo) => {
        element.carType = this.getEnumCarType(element.carType);
        newCars.push(element);
        this.notnullCar = true;
      });
      this.cars = newCars;
      this.spinnerService.hide();
      if (!this.notnullCar)
      {
          this.nullCar = true;
      }
    }, (err) => {
      this.spinnerService.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка автомобилей: ' + err.statusText);
    });
  }

  gotoCarOffices(carId: string) {
    this.router.navigate(['/office/car/', carId]);
  }

  deleteCar(carId: string) {
    this.apiService.deleteCar(carId).subscribe((res) => {
      this.notifier.notify('success', 'Автомобиль успешно удален');
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        'Ошибка удаления автомобиля: ' + err.statusText);
    });
    this.ngOnInit();  
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
