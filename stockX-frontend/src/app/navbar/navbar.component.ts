import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component, HostListener, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { UserService } from '../user.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, CommonModule, HttpClientModule],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  userService = inject(UserService)
  imagePath="cecLogo.png"
  isSidebarActive = false;

  constructor(private http: HttpClient, private router: Router) {}
  toggleSidebar() {
    this.isSidebarActive = !this.isSidebarActive;
  }

  logout(): void {
    this.userService.clearUser(); // Clear session storage
    this.router.navigate(['/']); // Redirect to the login page
  }

  @HostListener('window:resize', [])
  onWindowResize() {
    if (window.innerWidth > 768) {
      this.isSidebarActive = false; // Close sidebar on large screens
    }
  }
      ngOnInit(): void {

    }
}
