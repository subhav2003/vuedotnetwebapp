<template>
  <div class="flex flex-col min-h-screen bg-[#0f0f0f] text-[#C8B280]">
    <!-- Navbar -->
    <NavBar />

    <div class="container mx-auto px-4 py-6 flex flex-col gap-6">
      <!-- User Info -->
      <div class="rounded-lg p-6 w-full min-h-[400px] bg-[#1a1a1a] border border-[#333]">
        <UserInfo :user="user" @edit-profile="openEditForm" />
      </div>

      <!-- Orders -->
      <div class="rounded-lg p-6 w-full min-h-[400px] bg-[#1a1a1a] border border-[#333] overflow-y-auto">
        <OrderHistory @view-order="viewOrderDetails" />

        
      </div>
    </div>

    <!-- Modals -->
    <div v-if="selectedOrder" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60">
      <OrderDetails
          :order="selectedOrder"
          @close="closeOrderDetails"
          @review="writeReview"
      />
    </div>

    

    <div v-if="showReviewForm && selectedProduct" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60">
      <ReviewForm :product="selectedProduct" @close="closeReviewForm" />
    </div>

    <div v-if="showEditForm" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-60">
      <EditUserForm :user="user" @close="closeEditForm" @updated="handleUserUpdated" />
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
const selectedOrder = ref(null);
const selectedFinalizedOrder = ref(null);
const selectedProduct = ref(null);
const showReviewForm = ref(false);
const showEditForm = ref(false);

const isArtisan = computed(() => user.value?.role === 'artisan');

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

const viewOrderDetails = (order) => {
  selectedOrder.value = order;
  document.body.classList.add('no-scroll');
};
const closeOrderDetails = () => {
  selectedOrder.value = null;
  document.body.classList.remove('no-scroll');
};

const viewCustomInFinal = (order) => {
  selectedFinalizedOrder.value = order;
  document.body.classList.add('no-scroll');
};
const closeFinalizedOrder = () => {
  selectedFinalizedOrder.value = null;
  document.body.classList.remove('no-scroll');
};

const writeReview = (book) => {
  selectedProduct.value = book;
  showReviewForm.value = true;
};
const closeReviewForm = () => {
  showReviewForm.value = false;
  selectedProduct.value = null;
};

const openEditForm = () => { showEditForm.value = true; };
const closeEditForm = () => { showEditForm.value = false; };
const handleUserUpdated = (updated) => {
  user.value = updated;
  showEditForm.value = false;
};

onMounted(async () => {
  await fetchUser();
  if (isArtisan.value) await fetchCustomOrders();
});
</script>

<style scoped>
.no-scroll {
  overflow: hidden;
}
</style>
