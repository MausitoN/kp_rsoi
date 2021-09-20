import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { Router } from '@angular/router';
import { MatTableDataSource } from '@angular/material/table';
import { AuthService } from '../../services/auth-service';
import { MessageStreamService, StreamMessage } from '../../services/message-stream.service';

export interface ProfileBookingsInfo {
  bookingUid: string;
  registrationNumber: string;
  status: string;
  paymentUid: string;
  paymentStatus: string;
  price: string;
}

@Component({
  selector: 'app-profile-bookings',
  templateUrl: './profile-bookings.component.html',
  styleUrls: ['./profile-bookings.component.css']
})
export class ProfileBookingsComponent implements OnInit {
  private profileBookings: ProfileBookingsInfo[] = [];
  private userId: string;

  public displayedColumns: string[] = ['Uid', 'Number', 'Status', 'PaymentStatus','Price'];
  public dataSource = new MatTableDataSource<ProfileBookingsInfo>();

  private isLoggedIn = false;
  notnullBooking = false;
  nullBooking = false;
  private role = false;

  constructor(
    private apiService: ApiService,
    private messsageStream: MessageStreamService,
    private activateRoute: ActivatedRoute,
    private authService: AuthService,
    private router: Router,
    private notifier: NotifierService,
    private spinner: NgxSpinnerService
  ) { 
    this.isLoggedIn = this.authService.isLoggedIn();
    this.messsageStream.getMessage().subscribe((msg: StreamMessage) => {
      if (msg.type === 'login') {
        this.isLoggedIn = msg.data;
      }
    });
  }

  ngOnInit() {
    this.role = this.authService.isLoggedIn();
    if (!this.role)
    {
      this.router.navigate(['/login']);
    }
    this.spinner.show();
    this.apiService.getProfileBookings(this.authService.getUserId()).subscribe((res: ProfileBookingsInfo[]) => {
      res.forEach((element: ProfileBookingsInfo) => {
        element.status = this.getEnumBookingStatus(element.status);
        element.paymentStatus = this.getEnumPaymentStatus(element.paymentStatus);
        this.profileBookings.push(element);
        this.notnullBooking = true;
      });
      this.spinner.hide();
      if (!this.notnullBooking)
      {
          this.nullBooking = true;
      }
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения списка бронирования: ' + err.statusText);
    });
  }
  
  gotoProfileBooking(bookingId: string) {
    this.router.navigate(['/booking', bookingId]);
  }

  getEnumBookingStatus(value: string) {
    if (value == "NEW")
    {
      value = "Новый заказ"
    }
    else if (value == "FINISHED")
    {
      value = "Завершено"
    }
    else if (value == "CANCELED")
    {
      value = "Отменено"
    }
    else if (value == "EXPIRED")
    {
      value = "Истекла"
    }
    return value;
  }

  getEnumPaymentStatus(value: string) {
    if (value == "NEW")
    {
      value = "Новый заказ"
    }
    else if (value == "PAID")
    {
      value = "Оплачено"
    }
    else if (value == "CANCELED")
    {
      value = "Отменено"
    }
    else if (value == "REVERSED")
    {
      value = "Возвращено"
    }
    return value;
  }
}
