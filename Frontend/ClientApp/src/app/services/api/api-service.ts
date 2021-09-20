import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { BookingInfo } from '../../offices/office-car/office-car.component';
import { NewUserInfo } from '../../users/add-user/add-user.component';
import { NewCarInfo } from '../../cars/add-car/add-car.component';
import { AuthService } from '../auth-service';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  constructor(private http: HttpClient,
              private authService: AuthService
              ) { }

  login(username: string, password: string) {
    let authData = window.btoa(`${username}:${password}`);
    return this.http.post(`https://session-serv-carbookingsystem.herokuapp.com/auth`, null,
      {
        observe: 'response',
        headers: new HttpHeaders().append('Authorization', `Basic ${authData}`)
      });
  }

  public headers = new HttpHeaders()
  .set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
  

  getCars() {
    this.headers = new HttpHeaders()
      .set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`)
      .set('Access-Control-Allow-Origin', '*');
    return this.http.get("https://gw-carbookingsystem.herokuapp.com/cars", { 'headers': this.headers });
  }

  getOffices() {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get("https://gw-carbookingsystem.herokuapp.com/offices", { 'headers': this.headers });
  }

  getOfficeCars(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/office/${id}/cars`, { 'headers': this.headers });
  }

  getCarOffices(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/office/car/${id}`, { 'headers': this.headers });
  }

  getOfficeCar(idOffice: string, idCar: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/office/${idOffice}/car/${idCar}`, { 'headers': this.headers })
  }

  getBookingInfo(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/booking/${id}`, { 'headers': this.headers })
  }

  getProfileBookings(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/bookings/${id}`, { 'headers': this.headers })
  }

  getAllUsers() {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/users`, { 'headers': this.headers })
  }

  getReportModel() {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/report/booking-by-models`, { 'headers': this.headers })
  }

  getReportOffice() {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.get(`https://gw-carbookingsystem.herokuapp.com/report/booking-by-offices`, { 'headers': this.headers })
  }

  deleteCar(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.delete(`https://gw-carbookingsystem.herokuapp.com/car/${id}`, { 'headers': this.headers })
  }

  deleteCarFromOffice(idCar: string, idOffice: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.delete(`https://gw-carbookingsystem.herokuapp.com/office/${idOffice}/car/${idCar}`, { 'headers': this.headers })
  }

  createBooking(request: BookingInfo) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.post(`https://gw-carbookingsystem.herokuapp.com/booking`, request, { 'headers': this.headers })
  }

  addNewUser(request: NewUserInfo) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.post(`https://gw-carbookingsystem.herokuapp.com/user`, request, { 'headers': this.headers })
  }

  addNewCar(request: NewCarInfo) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.post(`https://gw-carbookingsystem.herokuapp.com/car`, request, { 'headers': this.headers })
  }

  addCarToOffice(idCar: string, idOffice: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.post(`https://gw-carbookingsystem.herokuapp.com/office/${idOffice}/car/${idCar}`, { 'headers': this.headers })
  }

  payBooking(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.post(`https://gw-carbookingsystem.herokuapp.com/payment/${id}/pay`, { 'headers': this.headers })
  }

  cancelBooking(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.delete(`https://gw-carbookingsystem.herokuapp.com/booking/${id}`, { 'headers': this.headers })
  }

  finishBooking(id: string) {
    this.headers = new HttpHeaders().set('Authorization', 'Bearer' + ` ${this.authService.getToken()}`);
    return this.http.post(`https://gw-carbookingsystem.herokuapp.com/booking/${id}/finish`, { 'headers': this.headers })
  }
}
