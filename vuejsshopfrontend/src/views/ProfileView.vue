<template>
  <div class="flex flex-col min-h-screen bg-gray-100">
    <!-- Navbar -->
    <NavBar />

    <div class="container mx-auto px-4 py-6 flex flex-col gap-6">
      <!-- User Information Section -->
      <div class="rounded-lg p-6 w-full min-h-[400px] bg-transparent">
        <!-- Listen for "edit-profile" event from UserInfo component -->
        <UserInfo :user="user" @edit-profile="openEditForm" />
      </div>

      <!-- Main Orders + Custom Orders Section -->
      <div class="rounded-lg p-6 w-full min-h-[400px] overflow-y-auto bg-transparent">
        <!-- Standard Order History -->
        <OrderHistory @view-order="viewOrderDetails" />

        <!-- Custom Order History (shown only if user is artisan and has custom orders) -->
        <div class="mt-8" >
          <!-- Now both view-details and view-finalization always open the FinalizedOrder overlay -->
          <CustomOrderHistory
              :orders="customOrders"
              @view-details="viewCustomInFinal"
              @view-finalization="viewCustomInFinal"
          />
        </div>
      </div>
    </div>

    <!-- Order Details Overlay (for non-finalized STANDARD orders) -->
    <div
        v-if="selectedOrder"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
    >
      <OrderDetails
          :order="selectedOrder"
          @close="closeOrderDetails"
          @write-review="writeReview"
      />
    </div>

    <!-- Finalized Order Overlay (now for ALL custom orders, plus any truly finalized) -->
    <div
        v-if="selectedFinalizedOrder"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60"
    >
      <FinalizedOrder
          :order="selectedFinalizedOrder"
          @close="closeFinalizedOrder"
      />
    </div>

    <!-- Review Form Overlay -->
    <div
        v-if="showReviewForm"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
    >
      <ReviewForm :product="selectedProduct" @close="closeReviewForm" />
    </div>

    <!-- Edit User Form Overlay -->
    <div
        v-if="showEditForm"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
    >
      <EditUserForm
          :user="user"
          @close="closeEditForm"
          @updated="handleUserUpdated"
      />
    </div>

    <!-- Footer -->
    <Footer class="mt-auto" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import NavBar from '@/components/NavBar.vue';
import Footer from '@/components/Footer.vue';
import OrderHistory from '@/components/OrderHistory.vue';
import CustomOrderHistory from '@/components/CustomOrderHistory.vue';
import OrderDetails from '@/components/OrderDetails.vue';
import ReviewForm from '@/components/ReviewForm.vue';
import UserInfo from '@/components/UserInfo.vue';
import EditUserForm from '@/components/EditUserForm.vue';
import FinalizedOrder from '@/components/FinalizedOrder.vue';

const user = ref(null);
const customOrders = ref([]);
const selectedOrder = ref(null);           // STANDARD order details
const selectedFinalizedOrder = ref(null);  // ALL custom orders + true finalized
const selectedProduct = ref(null);
const showReviewForm = ref(false);
const showEditForm = ref(false);

// Determine if current user is artisan
const isArtisan = computed(() => user.value?.role === 'artisan');

// Fetch user data
const fetchUser = async () => {
  try {
    const token = localStorage.getItem('authToken');
    const res = await fetch(`${process.env.VUE_APP_API_URL}/api/v1/user`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    user.value = await res.json();
  } catch (err) {
    console.error('Error fetching user:', err);
  }
};

// Fetch custom orders if artisan
const fetchCustomOrders = async () => {
  try {
    const token = localStorage.getItem('authToken');
    const res = await fetch(`${process.env.VUE_APP_API_URL}/api/v1/custom-orders`, {
      headers: { Authorization: `Bearer ${token}` },
    });
    const data = await res.json();
    customOrders.value = data.data || [];
  } catch (err) {
    console.error('Error fetching custom orders:', err);
  }
};

// Show the standard OrderDetails overlay
const viewOrderDetails = (order) => {
  selectedOrder.value = order;
  document.body.classList.add('no-scroll');
};
const closeOrderDetails = () => {
  selectedOrder.value = null;
  document.body.classList.remove('no-scroll');
};

// All custom orders open in the FinalizedOrder overlay
const viewCustomInFinal = (order) => {
  selectedFinalizedOrder.value = order;
  document.body.classList.add('no-scroll');
};
const closeFinalizedOrder = () => {
  selectedFinalizedOrder.value = null;
  document.body.classList.remove('no-scroll');
};

// Review form logic
const writeReview = (product) => {
  selectedProduct.value = product;
  showReviewForm.value = true;
};
const closeReviewForm = () => {
  showReviewForm.value = false;
  selectedProduct.value = null;
};

// Edit user form logic
const openEditForm = () => { showEditForm.value = true; };
const closeEditForm = () => { showEditForm.value = false; };
const handleUserUpdated = (updated) => {
  user.value = updated;
  showEditForm.value = false;
};

onMounted(async () => {
  await fetchUser();
  if (isArtisan.value) {
    await fetchCustomOrders();
  }
});
</script>

<style scoped>
.no-scroll {
  overflow: hidden;
}
.bg-transparent {
  background-color: transparent;
}
.min-h-[400px] {
  min-height: 400px;
}
</style>
