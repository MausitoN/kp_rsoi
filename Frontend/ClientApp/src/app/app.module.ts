import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NotifierModule } from 'angular-notifier';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { SignInComponent } from './auth/sign-in/sign-in.component';
import { AuthService } from './services/auth-service';
import { ApiService } from './services/api/api-service';
import { ApiInterceptor } from './services/api/api-interceptor';
import { AppRoutingModule } from './app-routing.module';
import { MessageStreamService } from './services/message-stream.service';
import { CarsComponent } from './cars/cars.component'; /**/
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgxSpinnerModule } from 'ngx-spinner';
import { OfficesComponent } from './offices/offices.component';
import { MaterialModule } from './material/material.module';
import { OfficeCarsComponent } from './offices/office-cars/office-cars.component';
import { CarOfficesComponent } from './cars/car-offices/car-offices.component';
import { OfficeCarComponent} from './offices/office-car/office-car.component';
import { BookingComponent} from './booking/booking.component';
import { ProfileBookingsComponent} from './booking/profile-bookings/profile-bookings.component';
import { UsersComponent} from './users/users.component'
import { AddUserComponent } from './users/add-user/add-user.component';
import { ReportModelComponent } from './reports/report-model/report-model.component';
import { ReportOfficeComponent } from './reports/report-office/report-office.component';
import { AddCarComponent } from './cars/add-car/add-car.component';
import { OfficeComponent } from './cars/add-car-offices/add-car-offices.component';
import { CarComponent } from './offices/add-office-cars/add-office-cars.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CarsComponent,
    SignInComponent,
    OfficesComponent,
    OfficeCarsComponent,
    CarOfficesComponent,
    OfficeCarComponent,
    BookingComponent,
    ProfileBookingsComponent,
    UsersComponent,
    AddUserComponent,
    ReportModelComponent,
    ReportOfficeComponent,
    AddCarComponent,
    OfficeComponent,
    CarComponent
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    BrowserAnimationsModule,
    FormsModule,
    NgxSpinnerModule,
    AppRoutingModule,
    MaterialModule,
    NotifierModule.withConfig({
      behaviour: {
        stacking: 2
      },
      position: {
        horizontal: {
          position: 'right',
        },
        vertical: {
          position: 'top',
          distance: 70
        }
      }
    }),
    BrowserAnimationsModule

  ],
  providers: [
    MessageStreamService,
    AuthService,
    ApiService, {
      provide: HTTP_INTERCEPTORS,
      useClass: ApiInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
