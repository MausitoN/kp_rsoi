import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpEvent, HttpInterceptor, HttpHandler, HttpRequest } from '@angular/common/http';
import { AuthService } from '../auth-service';
import { environment } from '../../../environments/environment';

@Injectable()
export class ApiInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) { }
  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.authService.isLoggedIn() && req.url.startsWith(environment.apiBaseURL)) {
      req = req.clone({
        setHeaders: {
          Authorization: 'Bearer' + ` ${this.authService.getToken()}`
        }
      });
    }
    return next.handle(req);
  }
}
