import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifierService } from 'angular-notifier';
import { NgxSpinnerService } from 'ngx-spinner';
import { ApiService } from '../../services/api/api-service';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

export interface ReportModelInfo {
  model: string;
  modelCount: string;
}

@Component({
  selector: 'app-report-model',
  templateUrl: './report-model.component.html',
  styleUrls: ['./report-model.component.css']
})

export class ReportModelComponent implements OnInit {
  private reportModel: ReportModelInfo[] = [];
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
    this.apiService.getReportModel().subscribe((res: ReportModelInfo[]) => {
      res.forEach((element: ReportModelInfo) => {
        this.reportModel.push(element);
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
        'Ошибка получения статистики по моделям: ' + err.statusText);
    });
  }
}
