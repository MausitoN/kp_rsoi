import { animate, state, style, transition, trigger } from '@angular/animations';
import { Component, Inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { from, Observable } from 'rxjs';
import { ApiService } from '../services/api/api-service';
import { NgxSpinnerService } from 'ngx-spinner';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { MessageStreamService, StreamMessage } from '../services/message-stream.service';

export interface BookingInfo { 
  registrationNumber: string;
  status: string;
  availabilitySchedule: string;
  brand: string;
  model: string;
  power: string;
  carType: string;
  locationFrom: string;
  locationTo: string;
  paymentUid: string;
  paymentStatus: string
  price: string;
}

@Component({
  selector: 'app-booking',
  styleUrls: ['./booking.component.css'],
  templateUrl: './booking.component.html',
})

export class BookingComponent implements OnInit {
  booking: BookingInfo;
  bookingId: string;

  private isLoggedIn = false;
  paid = false;
  cancel = false;
  finish = false;
  private role = false;

  constructor(
    private apiService: ApiService,
    private activateRoute: ActivatedRoute,
    private authService: AuthService,
    private messsageStream: MessageStreamService,
    private router: Router,
    private spinner: NgxSpinnerService,
    private notifier: NotifierService
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
    this.activateRoute.params.subscribe(params => this.bookingId = params['id']);
    this.apiService.getBookingInfo(this.bookingId).subscribe((res: BookingInfo) => {
      this.booking = res;

      if (this.booking.paymentStatus == "NEW")
      {
        this.paid = true;
      }
      if (this.booking.status == "NEW")
      {
        this.cancel = true;
        if (!this.paid)
        {
          this.finish = true;
        }
      }

      this.booking.status = this.getEnumBookingStatus(this.booking.status);
      this.booking.paymentStatus = this.getEnumPaymentStatus(this.booking.paymentStatus);
      this.booking.carType = this.getEnumCarType(this.booking.carType);

      this.spinner.hide();
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        '???????????? ?????????????????? ???????????????????? ???? ??????????: ' + err.statusText);
    });
  }

  payBooking(id: string) {
    this.apiService.payBooking(id).subscribe((res) => {
      this.notifier.notify('success', '?????????? ?????????????? ??????????????');
      this.ngOnInit();
      this.router.navigate(['/booking']);
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        '???????????? ????????????: ' + err.statusText);
    });
    this.ngOnInit();
    this.router.navigate(['/booking']);
  }

  cancelBooking(id: string) {
    this.apiService.cancelBooking(id).subscribe((res) => {
      this.notifier.notify('success', '???????????????????????? ????????????????');
      this.ngOnInit();
      this.router.navigate(['/booking']);
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        '???????????? ????????????: ' + err.statusText);
    });
    this.ngOnInit();
    this.router.navigate(['/booking']);
  }

  finishBooking(id: string) {
    this.apiService.finishBooking(id).subscribe((res) => {
      this.notifier.notify('success', '???????????????????????? ??????????????????');
      this.ngOnInit();
      this.router.navigate(['/booking']);
    }, (err) => {
      console.log(err);
      this.notifier.notify('error',
        '???????????? ????????????????????: ' + err.statusText);
    });
    this.ngOnInit();
    this.router.navigate(['/booking']);
  }

  getEnumBookingStatus(value: string) {
    if (value == "NEW")
    {
      value = "?????????? ??????????"
    }
    else if (value == "FINISHED")
    {
      value = "??????????????????"
    }
    else if (value == "CANCELED")
    {
      value = "????????????????"
    }
    else if (value == "EXPIRED")
    {
      value = "??????????????"
    }
    return value;
  }

  getEnumPaymentStatus(value: string) {
    if (value == "NEW")
    {
      value = "?????????? ??????????"
    }
    else if (value == "PAID")
    {
      value = "????????????????"
    }
    else if (value == "CANCELED")
    {
      value = "????????????????"
    }
    else if (value == "REVERSED")
    {
      value = "????????????????????"
    }
    return value;
  }

  getEnumCarType(value: string) {
    if (value == "SEDAN")
    {
      value = "??????????"
    }
    else if (value == "SUV")
    {
      value = "??????????????????????"
    }
    else if (value == "MINIVAN")
    {
      value = "??????????????"
    }
    else if (value == "ROADSTER")
    {
      value = "??????????????"
    }
    return value;
  }
}
