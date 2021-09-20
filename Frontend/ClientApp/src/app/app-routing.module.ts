import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SignInComponent } from './auth/sign-in/sign-in.component';
import { CarsComponent } from './cars/cars.component';
import { HomeComponent } from './home/home.component';
import { OfficesComponent } from './offices/offices.component';
import { OfficeCarsComponent } from './offices/office-cars/office-cars.component';
import { CarOfficesComponent } from './cars/car-offices/car-offices.component';
import { OfficeCarComponent } from './offices/office-car/office-car.component';
import { BookingComponent } from './booking/booking.component';
import { ProfileBookingsComponent } from './booking/profile-bookings/profile-bookings.component';
import { UsersComponent } from './users/users.component';
import { AddUserComponent } from './users/add-user/add-user.component';
import { ReportModelComponent } from './reports/report-model/report-model.component';
import { ReportOfficeComponent } from './reports/report-office/report-office.component';
import { AddCarComponent } from './cars/add-car/add-car.component';
import { OfficeComponent } from './cars/add-car-offices/add-car-offices.component';
import { CarComponent } from './offices/add-office-cars/add-office-cars.component';


const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'cars', component: CarsComponent },
  { path: 'office/:id/cars', component: OfficeCarsComponent },
  { path: 'office/car/:id', component: CarOfficesComponent },
  { path: 'office/:idOffice/car/:idCar', component: OfficeCarComponent },
  { path: 'offices', component: OfficesComponent },
  { path: 'booking/:id', component: BookingComponent },
  { path: 'booking', component: ProfileBookingsComponent },
  { path: 'users', component: UsersComponent },
  { path: 'add-user', component: AddUserComponent },
  { path: 'reports/booking-by-models', component: ReportModelComponent },
  { path: 'reports/booking-by-offices', component: ReportOfficeComponent },
  { path: 'car/add-new-car', component: AddCarComponent },
  { path: 'office/add-car-offices/:id', component: OfficeComponent },
  { path: 'office/add-office-cars/:id', component: CarComponent },
  { path: 'login', component: SignInComponent }
];

@NgModule({
  imports: [
    RouterModule.forRoot(routes, { relativeLinkResolution: 'legacy' })
  ],
  exports: [
    RouterModule
  ]
})

export class AppRoutingModule { }
