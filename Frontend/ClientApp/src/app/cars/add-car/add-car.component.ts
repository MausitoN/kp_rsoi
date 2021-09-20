import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface NewCarInfo {
  Brand: string;
  Model: string;
  Power: string;
  CarType: string;
}

@Component({
  selector: 'app-add-car',
  templateUrl: './add-car.component.html',
  styleUrls: ['./add-car.component.css']
})

export class AddCarComponent implements OnInit {
  automobileTypes = [
    { id: "SEDAN", name: "Седан" },
    { id: "SUV", name: "Внедорожник"},
    { id: "MINIVAN", name: "Минивэн" },
    { id: "ROADSTER", name: "Родстер" }    
  ]

  brand = "";
  model = "";
  power = "";
  private carTypeId = "SEDAN";

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

  selectCarType(event: any): void {
    this.carTypeId = event.target.value;
  }

  addCar() {
    let request: NewCarInfo = {
      Brand: this.brand,
      Model: this.model,
      Power: this.power,
      CarType: this.carTypeId };

    this.apiService.addNewCar(request).subscribe((res) => {
      this.notifier.notify('success', 'Автомобиль успешно добавлен');
      this.router.navigate(['/cars']);
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка пользователей: ' + err.statusText);
    });
    this.router.navigate(['/cars']);
  }
}
