import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { ApiService } from '../services/api/api-service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})

export class HomeComponent {
	
  logIs = false;
  role = "";
  profile = false;
  constructor(private authService: AuthService,
  			     private router: Router) { 
  	if (authService.getToken() != null)
  	{
  		this.logIs = true;
      this.role = this.authService.getUserRole();
      this.profile = true;
  	}
    if (this.role == "admin")
    {
      this.profile = false;
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
