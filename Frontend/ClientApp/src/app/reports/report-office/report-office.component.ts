import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface ReportOfficeInfo {
  office: string;
  locationCount: string;
}

@Component({
  selector: 'app-report-office',
  templateUrl: './report-office.component.html',
  styleUrls: ['./report-office.component.css']
})

export class ReportOfficeComponent implements OnInit {
  private reportOffice: ReportOfficeInfo[] = [];
  notnullReport = false;
  nullReport = false;

  constructor(
    private apiService: ApiService,
    private activateRoute: ActivatedRoute,
    private notifier: NotifierService,
    private spinner: NgxSpinnerService,
    private router: Router,
    private authService: AuthService
  ) { }
  
  ngOnInit() {
    if (this.authService.getUserRole() != 'admin')
    {
      this.router.navigate(['/login']);
    }
    this.spinner.show();
    this.apiService.getReportOffice().subscribe((res: ReportOfficeInfo[]) => {
      res.forEach((element: ReportOfficeInfo) => {
        this.reportOffice.push(element);
        this.notnullReport = true;
      });
      this.spinner.hide();
      if (!this.notnullReport)
      {
          this.nullReport = true;
      }
    }, (err) => {
      this.spinner.hide();
      console.log(err);
      this.notifier.notify('error',
        'Ошибка получения статистики по офисам бронирования: ' + err.statusText);
    });
  }
}
