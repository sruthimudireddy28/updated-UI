import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../../services/hotel.service';
import { ReviewService } from '../../services/review.service';
import { BookingService } from '../../services/booking.service';
import { AuthService } from '../../core/services/auth.service';
import { AnalyticsStatsComponent } from './analytics-stats/analytics-stats.component';
import { HotelFormManagerComponent } from './hotel-form-manager/hotel-form-manager.component';
import { RoomFormManagerComponent } from './room-form-manager/room-form-manager.component';
import { ReviewModeratorComponent } from './review-moderator/review-moderator.component';

@Component({
  selector: 'app-admin-panel',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    AnalyticsStatsComponent, 
    HotelFormManagerComponent, 
    RoomFormManagerComponent, 
    ReviewModeratorComponent
  ],
  templateUrl: './admin-panel.component.html',
  styleUrl: './admin-panel.component.css'
})
export class AdminPanelComponent implements OnInit {
  hotelService = inject(HotelService);
  reviewService = inject(ReviewService);
  bookingService = inject(BookingService);
  auth = inject(AuthService);

  activeTab = signal<'hotels' | 'reviews' | 'bookings' | 'approvals'>('hotels');
  
  // Create Hotel Form
  isSubmittingHotel = signal(false);
  hotelForm = {
    name: '',
    location: '',
    description: '',
    amenities: 'WiFi, Pool, Gym, Spa, Parking'
  };

  // Create Room Form
  isSubmittingRoom = signal(false);
  roomForm = {
    hotelId: 0,
    roomType: 'Double Room',
    pricePerNight: 5000,
    amenities: 'AC, Flat-screen TV, Balcony, Mini Fridge',
    roomNumber: ''
  };

  // Reviews responding
  selectedHotelIdForReviews = 0;
  reviewResponses: { [key: number]: string } = {};

  // Manager Approval & Assignments
  managers = signal<any[]>([]);
  selectedManagerForAssignment = signal<any | null>(null);
  assignedHotelIds = signal<number[]>([]);
  allHotels = signal<any[]>([]);

  // Edit Manager
  selectedManagerForEdit = signal<any | null>(null);
  editManagerForm = {
    name: '',
    contactNumber: ''
  };
  editAssignedHotelIds = signal<number[]>([]);
  isSavingManagerEdit = signal(false);

  ngOnInit() {
    if (this.auth.isAdmin()) {
      this.hotelService.getAllHotels().subscribe();
      this.bookingService.getAllBookings().subscribe();
      this.loadManagers();
      this.activeTab.set('approvals'); // Make approvals default tab for Admins
    } else if (this.auth.isManager()) {
      this.hotelService.getMyHotels().subscribe();
      this.bookingService.getAllBookings().subscribe();
    }
  }

  loadManagers() {
    this.auth.getManagers().subscribe({
      next: (res) => {
        if (res.success) {
          this.managers.set(res.data || []);
        }
      }
    });
  }

  approveManager(id: number) {
    this.auth.updateManagerStatus(id, 'Approved').subscribe({
      next: (res) => {
        if (res.success) {
          this.loadManagers();
          // Open assignment dialog directly upon approval
          this.openAssignmentModal(res.data);
        }
      }
    });
  }

  rejectManager(id: number) {
    if (confirm('Are you sure you want to reject this manager application?')) {
      this.auth.updateManagerStatus(id, 'Rejected').subscribe({
        next: () => this.loadManagers()
      });
    }
  }

  openAssignmentModal(manager: any) {
    this.selectedManagerForAssignment.set(manager);
    // Load all hotels for selection
    this.hotelService.getAllHotels().subscribe({
      next: (res) => {
        if (res.success) {
          this.allHotels.set(res.data || []);
        }
      }
    });
    // Load hotels currently managed by this manager
    this.hotelService.getHotelsByManager(manager.userId).subscribe({
      next: (res) => {
        if (res.success) {
          const list = res.data || [];
          this.assignedHotelIds.set(list.map((h: any) => h.hotelId));
        } else {
          this.assignedHotelIds.set([]);
        }
      }
    });
  }

  toggleHotelSelection(hotelId: number) {
    const current = this.assignedHotelIds();
    if (current.includes(hotelId)) {
      this.assignedHotelIds.set(current.filter(id => id !== hotelId));
    } else {
      this.assignedHotelIds.set([...current, hotelId]);
    }
  }

  saveAssignments() {
    const manager = this.selectedManagerForAssignment();
    if (!manager) return;

    this.hotelService.assignHotelsToManager(manager.userId, this.assignedHotelIds()).subscribe({
      next: (res) => {
        if (res.success) {
          this.selectedManagerForAssignment.set(null);
          this.loadManagers();
          if (this.auth.isAdmin()) {
            this.hotelService.getAllHotels().subscribe();
          } else {
            this.hotelService.getMyHotels().subscribe();
          }
        }
      }
    });
  }

  openEditManagerModal(manager: any) {
    this.selectedManagerForEdit.set(manager);
    this.editManagerForm.name = manager.name || '';
    this.editManagerForm.contactNumber = manager.contactNumber || '';

    // Load all hotels for selection
    this.hotelService.getAllHotels().subscribe({
      next: (res) => {
        if (res.success) {
          this.allHotels.set(res.data || []);
        }
      }
    });
    // Load hotels currently managed by this manager
    this.hotelService.getHotelsByManager(manager.userId).subscribe({
      next: (res) => {
        if (res.success) {
          const list = res.data || [];
          this.editAssignedHotelIds.set(list.map((h: any) => h.hotelId));
        } else {
          this.editAssignedHotelIds.set([]);
        }
      }
    });
  }

  toggleEditHotelSelection(hotelId: number) {
    const current = this.editAssignedHotelIds();
    if (current.includes(hotelId)) {
      this.editAssignedHotelIds.set(current.filter(id => id !== hotelId));
    } else {
      this.editAssignedHotelIds.set([...current, hotelId]);
    }
  }

  saveManagerEdits() {
    const manager = this.selectedManagerForEdit();
    if (!manager) return;
    if (this.isSavingManagerEdit()) return;

    this.isSavingManagerEdit.set(true);

    // Update manager profile (name, contact)
    const profilePayload: any = {
      name: this.editManagerForm.name,
      contactNumber: this.editManagerForm.contactNumber
    };

    this.auth.updateProfile(manager.userId, profilePayload).subscribe({
      next: () => {
        // After profile update, save hotel assignments
        this.hotelService.assignHotelsToManager(manager.userId, this.editAssignedHotelIds()).subscribe({
          next: (res) => {
            this.isSavingManagerEdit.set(false);
            if (res.success) {
              this.selectedManagerForEdit.set(null);
              this.loadManagers();
              this.hotelService.getAllHotels().subscribe();
            }
          },
          error: () => {
            this.isSavingManagerEdit.set(false);
          }
        });
      },
      error: () => {
        this.isSavingManagerEdit.set(false);
      }
    });
  }

  getManagerStatusClass(status: string): string {
    if (status === 'Approved') return 'badge-success';
    if (status === 'Pending') return 'badge-warning';
    return 'badge-danger';
  }

  submitHotel() {
    this.isSubmittingHotel.set(true);
    this.hotelService.createHotel(this.hotelForm).subscribe({
      next: () => {
        this.isSubmittingHotel.set(false);
        this.hotelForm = {
          name: '',
          location: '',
          description: '',
          amenities: 'WiFi, Pool, Gym, Spa, Parking'
        };
        if (this.auth.isAdmin()) {
          this.hotelService.getAllHotels().subscribe();
        } else {
          this.hotelService.getMyHotels().subscribe();
        }
      },
      error: () => this.isSubmittingHotel.set(false)
    });
  }

  submitRoom() {
  if (this.roomForm.hotelId === 0) {
    alert('Please select a hotel first');
    return;
  }

  this.isSubmittingRoom.set(true);

  const roomPayload = {
    hotelId: Number(this.roomForm.hotelId),
    roomNumber: String(this.roomForm.roomNumber),
    roomType: this.roomForm.roomType,
    pricePerNight: Number(this.roomForm.pricePerNight),
    description: "Standard room description",
    maxOccupancy: 2,
    bedCount: 1,
    bedType: "Queen",
    floorNumber: 1,
    roomSize: 250,
    imageUrl: "",
    imageUrls: [],
    amenities: [] 
  };

  this.hotelService.createRoom(roomPayload).subscribe({
    next: (response) => {
      this.isSubmittingRoom.set(false);
      alert('Room added successfully!');
      this.resetRoomForm();
    },
    error: (err) => {
      this.isSubmittingRoom.set(false);
      
      // Check if it's actually a duplicate database error
      if (err.error && err.error.includes("duplicate key row")) {
        alert(`Room number ${this.roomForm.roomNumber} already exists in this hotel! Please use a different room number.`);
        return;
      }

      // If the database added it but the client choked on the 201 response object structure:
      if (err.status === 201 || err.status === 200) {
        alert('Room added successfully!');
        this.resetRoomForm();
      } else {
        console.error("Full Server Error Details:", err);
        alert(`Server returned an error (${err.status}). Check console logs.`);
      }
    }
  });
}

// Helper method to cleanly reset the form state
resetRoomForm() {
  this.roomForm = {
    hotelId: this.roomForm.hotelId,
    roomType: 'Double Room',
    pricePerNight: 5000,
    amenities: 'AC, Flat-screen TV, Balcony, Mini Fridge',
    roomNumber: ''
  };
}

  loadReviewsForHotel() {
    if (this.selectedHotelIdForReviews > 0) {
      this.reviewService.getHotelReviews(this.selectedHotelIdForReviews).subscribe();
    }
  }

  submitResponse(reviewId: number) {
    const text = this.reviewResponses[reviewId];
    if (!text || text.trim().length === 0) return;

    this.reviewService.respondToReview(reviewId, text, this.selectedHotelIdForReviews).subscribe({
      next: () => {
        delete this.reviewResponses[reviewId];
      }
    });
  }

  getBookingStatusClass(status: string): string {
    if (status === 'Confirmed' || status === 'Completed' || status === 'Paid') return 'badge-success';
    if (status === 'Pending') return 'badge-warning';
    return 'badge-danger';
  }
}
