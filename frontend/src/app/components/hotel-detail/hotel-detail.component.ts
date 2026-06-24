import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { HotelService } from '../../services/hotel.service';
import { ReviewService } from '../../services/review.service';
import { BookingService } from '../../services/booking.service';
import { AuthService } from '../../core/services/auth.service';
import { RoomCardComponent } from './room-card/room-card.component';
import { ReviewCardComponent } from './review-card/review-card.component';
import { BookingFormDialogComponent } from './booking-form-dialog/booking-form-dialog.component';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Component({
  selector: 'app-hotel-detail',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    RouterModule, 
    RoomCardComponent, 
    ReviewCardComponent, 
    BookingFormDialogComponent
  ],
  templateUrl: './hotel-detail.component.html',
  styleUrl: './hotel-detail.component.css'
})
export class HotelDetailComponent implements OnInit {
  hotelService = inject(HotelService);
  reviewService = inject(ReviewService);
  bookingService = inject(BookingService);
  auth = inject(AuthService);
  private http = inject(HttpClient);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  hotelId = 0;
  minDate = '';
  minCheckOutDate = '';
  
  dates = {
    checkIn: '',
    checkOut: ''
  };

  // Local signal to store only verified available rooms
  availableRooms = signal<any[]>([]);

  // Reservation dialog
  activeBookingRoom = signal<any | null>(null);
  isReserving = signal(false);
  bookingForm = {
    guestName: '',
    guestEmail: '',
    guestPhone: '',
    numberOfGuests: 1,
    specialRequests: ''
  };

  ngOnInit() {
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];
    
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);
    this.minCheckOutDate = tomorrow.toISOString().split('T')[0];

    this.dates.checkIn = this.minDate;
    this.dates.checkOut = this.minCheckOutDate;

    this.route.params.subscribe(params => {
      this.hotelId = parseInt(params['id'], 10);
      if (this.hotelId) {
        this.loadHotelDetails();
      }
    });
  }

  loadHotelDetails() {
    this.hotelService.getHotelById(this.hotelId).subscribe();
    this.onSearchRooms();
    this.reviewService.getHotelReviews(this.hotelId).subscribe();
    this.reviewService.getHotelSummary(this.hotelId).subscribe();
  }

  onDateChange() {
    if (this.dates.checkIn) {
      const checkInDate = new Date(this.dates.checkIn);
      const nextDay = new Date(checkInDate);
      nextDay.setDate(checkInDate.getDate() + 1);
      this.minCheckOutDate = nextDay.toISOString().split('T')[0];

      if (this.dates.checkOut <= this.dates.checkIn) {
        this.dates.checkOut = this.minCheckOutDate;
      }

      this.onSearchRooms();
    }
  }

  onSearchRooms() {
  this.hotelService.getAvailableRooms(
    this.hotelId, 
    this.dates.checkIn, 
    this.dates.checkOut
  ).subscribe((roomsResponse: any) => {
    
    const rooms = roomsResponse?.data || roomsResponse || [];
    
    if (!rooms.length) {
      this.availableRooms.set([]);
      return;
    }

    const checkRequests = rooms.map((room: any) => {
      // ✅ FIX: Using the correct gateway URL via bookingService
      return this.bookingService.checkRoomAvailability(
        room.roomId, 
        this.dates.checkIn, 
        this.dates.checkOut
      ).pipe(
        // For safety, if the server checks fail, default to false (not available)
        catchError(() => of({ data: false })) 
      );
    });

    forkJoin(checkRequests).subscribe((results: any) => {
      const filtered = rooms.filter((room: any, index: number) => {
        const res = results[index];
        // Only return true if the backend explicitly says the room is available
        return res?.data === true || res === true;
      });

      this.availableRooms.set(filtered);
    });
  });
}

  getNightsCount(): number {
    if (!this.dates.checkIn || !this.dates.checkOut) return 1;
    const start = new Date(this.dates.checkIn);
    const end = new Date(this.dates.checkOut);
    const diff = end.getTime() - start.getTime();
    const nights = Math.ceil(diff / (1000 * 60 * 60 * 24));
    return nights <= 0 ? 1 : nights;
  }

  bookRoom(room: any) {
    if (!this.auth.isLoggedIn()) {
      this.router.navigate(['/auth']);
      return;
    }

    this.activeBookingRoom.set(room);
    this.bookingForm = {
      guestName: this.auth.currentUser()?.name || '',
      guestEmail: this.auth.currentUser()?.email || '',
      guestPhone: '',
      numberOfGuests: 1,
      specialRequests: ''
    };
  }

  
submitReservation() {
  const room = this.activeBookingRoom();
  if (!room) return;

  if (this.isReserving()) {
    return;
  }

  this.isReserving.set(true);

  // ✅ FIX: Send raw string dates directly to avoid UTC day-shifting bugs
  const payload = {
    roomId: room.roomId,
    hotelId: this.hotelId,
    checkInDate: this.dates.checkIn, 
    checkOutDate: this.dates.checkOut,
    numberOfGuests: this.bookingForm.numberOfGuests,
    specialRequests: this.bookingForm.specialRequests,
    guestName: this.bookingForm.guestName,
    guestEmail: this.bookingForm.guestEmail,
    guestPhone: this.bookingForm.guestPhone
  };

  this.bookingService.createBooking(payload).subscribe({
    next: () => {
      this.isReserving.set(false);
      this.activeBookingRoom.set(null);
      this.router.navigate(['/dashboard']);
    },
    error: () => {
      this.isReserving.set(false);
    }
  });
}

  markHelpful(reviewId: number) {
    this.reviewService.markHelpful(reviewId, this.hotelId).subscribe();
  }

  getHotelGradient(id: number): string {
    const gradients = [
      'linear-gradient(135deg, #4338ca 0%, #1e1b4b 100%)',
      'linear-gradient(135deg, #6d28d9 0%, #311042 100%)',
      'linear-gradient(135deg, #0f766e 0%, #042f2e 100%)',
      'linear-gradient(135deg, #b91c1c 0%, #450a0a 100%)',
      'linear-gradient(135deg, #1d4ed8 0%, #172554 100%)'
    ];
    return gradients[id % gradients.length];
  }

  getAmenities(amenities: any[] | string | null): string[] {
    if (!amenities) {
      return ['WiFi', 'AC', 'TV']; 
    }

    if (Array.isArray(amenities)) {
      return amenities.map(a => {
        if (typeof a === 'object' && a !== null) {
          return a.name; 
        }
        return String(a);
      });
    }

    if (typeof amenities === 'string') {
      return amenities.split(',')
        .map(s => s.trim())
        .filter(s => s.length > 0);
    }

    return [];
  }

  getStarRows() {
    const summary = this.reviewService.summaries();
    if (!summary) return [];

    const total = summary.totalReviews || 1;
    return [
      { stars: 5, count: summary.fiveStarCount, percent: (summary.fiveStarCount / total) * 100 },
      { stars: 4, count: summary.fourStarCount, percent: (summary.fourStarCount / total) * 100 },
      { stars: 3, count: summary.threeStarCount, percent: (summary.threeStarCount / total) * 100 },
      { stars: 2, count: summary.twoStarCount, percent: (summary.twoStarCount / total) * 100 },
      { stars: 1, count: summary.oneStarCount, percent: (summary.oneStarCount / total) * 100 }
    ];
  }

  // Admin / Manager control checks
  canManage(): boolean {
    if (!this.auth.isLoggedIn()) return false;
    if (this.auth.isAdmin()) return true;
    const current = this.hotelService.currentHotel();
    return current && current.managerId === this.auth.currentUser()?.userId;
  }

  isEditingHotel = signal(false);
  editHotelForm = {
    name: '',
    location: '',
    address: '',
    city: '',
    state: '',
    country: '',
    zipCode: '',
    description: '',
    contactNumber: '',
    email: '',
    imageUrl: '',
    amenities: ''
  };

  editingRoom = signal<any | null>(null);
  editRoomForm = {
    roomNumber: '',
    roomType: '',
    pricePerNight: 0,
    description: '',
    maxOccupancy: 2,
    bedCount: 1,
    bedType: 'Queen',
    floorNumber: 1,
    roomSize: 250,
    imageUrl: '',
    isAvailable: true
  };

  openEditHotelModal() {
    const hotel = this.hotelService.currentHotel();
    if (!hotel) return;

    this.editHotelForm = {
      name: hotel.name || '',
      location: hotel.location || '',
      address: hotel.address || '',
      city: hotel.city || '',
      state: hotel.state || '',
      country: hotel.country || '',
      zipCode: hotel.zipCode || '',
      description: hotel.description || '',
      contactNumber: hotel.contactNumber || '',
      email: hotel.email || '',
      imageUrl: hotel.imageUrl || '',
      amenities: (hotel.amenities || []).map((a: any) => a.name || a).join(', ')
    };
    this.isEditingHotel.set(true);
  }

  submitEditHotel() {
    const payload = {
      name: this.editHotelForm.name,
      location: this.editHotelForm.location,
      address: this.editHotelForm.address,
      city: this.editHotelForm.city,
      state: this.editHotelForm.state,
      country: this.editHotelForm.country,
      zipCode: this.editHotelForm.zipCode,
      description: this.editHotelForm.description,
      contactNumber: this.editHotelForm.contactNumber,
      email: this.editHotelForm.email,
      imageUrl: this.editHotelForm.imageUrl,
      amenityIds: this.editHotelForm.amenities
        .split(',')
        .map(s => s.trim())
        .filter(s => s.length > 0)
        .map(s => this.mapAmenityNameToId(s))
        .filter(id => id > 0)
    };

    this.hotelService.updateHotel(this.hotelId, payload).subscribe({
      next: () => {
        this.isEditingHotel.set(false);
        this.loadHotelDetails();
      }
    });
  }

  onDeleteHotel() {
    if (!confirm('Are you sure you want to delete this hotel? This action cannot be undone.')) {
      return;
    }

    this.hotelService.deleteHotel(this.hotelId).subscribe({
      next: () => {
        this.router.navigate(['/']);
      }
    });
  }

  openEditRoomModal(room: any) {
    this.editingRoom.set(room);
    this.editRoomForm = {
      roomNumber: room.roomNumber || '',
      roomType: room.roomType || '',
      pricePerNight: room.pricePerNight || room.price || 0,
      description: room.description || '',
      maxOccupancy: room.maxOccupancy || 2,
      bedCount: room.bedCount || 1,
      bedType: room.bedType || 'Queen',
      floorNumber: room.floorNumber || 1,
      roomSize: room.roomSize || 250,
      imageUrl: room.imageUrl || '',
      isAvailable: room.isAvailable !== false
    };
  }

  submitEditRoom() {
    const room = this.editingRoom();
    if (!room) return;

    const payload = {
      roomNumber: String(this.editRoomForm.roomNumber),
      roomType: this.editRoomForm.roomType,
      pricePerNight: Number(this.editRoomForm.pricePerNight),
      description: this.editRoomForm.description,
      maxOccupancy: Number(this.editRoomForm.maxOccupancy),
      bedCount: Number(this.editRoomForm.bedCount),
      bedType: this.editRoomForm.bedType,
      floorNumber: Number(this.editRoomForm.floorNumber),
      roomSize: Number(this.editRoomForm.roomSize),
      imageUrl: this.editRoomForm.imageUrl,
      isAvailable: this.editRoomForm.isAvailable,
      hotelId: this.hotelId
    };

    this.hotelService.updateRoom(room.roomId, payload).subscribe({
      next: () => {
        this.editingRoom.set(null);
        this.onSearchRooms();
      }
    });
  }

  onDeleteRoom(room: any) {
    if (!confirm(`Are you sure you want to delete room ${room.roomNumber}?`)) {
      return;
    }

    this.hotelService.deleteRoom(room.roomId, this.hotelId).subscribe({
      next: () => {
        this.onSearchRooms();
      }
    });
  }

  mapAmenityNameToId(name: string): number {
    const map: { [key: string]: number } = {
      'wifi': 1, 'free wifi': 1,
      'pool': 2, 'swimming pool': 2,
      'gym': 3, 'fitness center': 3,
      'spa': 4,
      'restaurant': 5,
      'bar': 6,
      'parking': 7, 'free parking': 7,
      'room service': 8,
      'laundry': 9,
      'airport shuttle': 10,
      'business center': 11,
      'conference room': 12,
      'pet friendly': 13,
      'air conditioning': 14, 'ac': 14,
      'front desk': 15, '24/7 front desk': 15
    };
    return map[name.toLowerCase().trim()] || 0;
  }
}