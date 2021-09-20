import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface CarInfo {
  id: string;
  brand: string;
  model: string;
  power: string;
  carType: string;
}

@Component({
  selector: 'app-add-office-cars',
  templateUrl: './add-office-cars.component.html',
  styleUrls: ['./add-office-cars.component.css']
})
export class CarComponent implements OnInit {
  private cars: CarInfo[] = [];
  private officeId: string;

  notnullCar = false;
  nullCar = false;

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
    this.activateRoute.params.subscribe(params => this.officeId = params['id']);
    this.spinner.show();
    this.apiService.getCars().subscribe((res: CarInfo[]) => {
      res.forEach((element: CarInfo) => {
        element.carType = this.getEnumCarType(element.carType);
        this.cars.push(element);
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
        'Ошибка получения списка автомобилей: ' + err.statusText);
    });
  }

  addCarToOffice(carId: string, officeId: string) {
    this.apiService.addCarToOffice(carId, officeId).subscribe((res) => {
      this.notifier.notify('success', 'Автомобиль успешно добавлен в офис');
      this.router.navigate(['office', officeId, 'car']);
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        'Ошибка добавления автомобиля в офис: ' + err.statusText);
    });
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
}
