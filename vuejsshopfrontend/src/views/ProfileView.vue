<template>
  <div class="flex flex-col min-h-screen bg-gray-100">
    <!-- Navbar -->
    <NavBar />

    <div class="container mx-auto px-4 py-6 flex flex-col gap-6">
      <!-- User Information Section -->
      <div class="rounded-lg p-6 w-full min-h-[400px] bg-transparent">
        <!-- Show profile & edit button -->
        <UserInfo :user="user" @edit-profile="openEditForm" />
      </div>

      <!-- Main Orders + Custom Orders Section -->
      <div class="rounded-lg p-6 w-full min-h-[400px] overflow-y-auto bg-transparent">
        <!-- Standard Order History -->
        <OrderHistory @view-order="viewOrderDetails" />

        <!-- Custom Order History (artisans only) -->
        <CustomOrderHistory
            v-if="isArtisan && customOrders.length"
            :orders="customOrders"
            @view-custom-order="viewCustomInFinal"
        />
      </div>
    </div>

    <!-- Standard Order Details Overlay -->
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

    <!-- Finalized Order Overlay (for custom or truly finalized) -->
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
      <ReviewForm
          :product="selectedProduct"
          @close="closeReviewForm"
      />
    </div>

    <!-- Edit Profile Overlay -->
    <div
        v-if="showEditForm"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
    >
      <EditProfile
          :user="user"
          @close="closeEditForm"
          @updated="handleProfileUpdated"
      />
    </div>

    <!-- Footer -->
    <Footer class="mt-auto" />
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import NavBar from '@/components/NavBar.vue'
import Footer from '@/components/Footer.vue'
import OrderHistory from '@/components/OrderHistory.vue'
import CustomOrderHistory from '@/components/CustomOrderHistory.vue'
import OrderDetails from '@/components/OrderDetails.vue'
import FinalizedOrder from '@/components/FinalizedOrder.vue'
import ReviewForm from '@/components/ReviewForm.vue'
import UserInfo from '@/components/UserInfo.vue'
import EditProfile from '@/components/EditUserForm.vue'

const api = process.env.VUE_APP_API_URL || 'https://localhost:5001'
const token = localStorage.getItem('authToken')

// state
const user                  = ref(null)
const customOrders          = ref([])
const selectedOrder         = ref(null)
const selectedFinalizedOrder= ref(null)
const selectedProduct       = ref(null)
const showReviewForm        = ref(false)
const showEditForm          = ref(false)

// artisan check
const isArtisan = computed(() => user.value?.role === 'artisan')

// Fetch user profile
async function fetchUser() {
  try {
    const res = await fetch(`${api}/api/account/profile`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    const data = await res.json()
    user.value = data.user
  } catch (err) {
    console.error('Error fetching user profile:', err)
  }
}

// Fetch custom orders for artisans
async function fetchCustomOrders() {
  if (!isArtisan.value) return
  try {
    const res = await fetch(`${api}/api/v1/custom-orders`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    const json = await res.json()
    customOrders.value = json.data || []
  } catch (err) {
    console.error('Error fetching custom orders:', err)
  }
}

// ── Order overlays ────────────────────────────────────────────────────────
const viewOrderDetails = order => {
  selectedOrder.value = order
  document.body.classList.add('no-scroll')
}
const closeOrderDetails = () => {
  selectedOrder.value = null
  document.body.classList.remove('no-scroll')
}

const viewCustomInFinal = order => {
  selectedFinalizedOrder.value = order
  document.body.classList.add('no-scroll')
}
const closeFinalizedOrder = () => {
  selectedFinalizedOrder.value = null
  document.body.classList.remove('no-scroll')
}

// ── Review form ────────────────────────────────────────────────────────────
const writeReview = product => {
  selectedProduct.value = product
  showReviewForm.value = true
}
const closeReviewForm = () => {
  showReviewForm.value = false
  selectedProduct.value = null
}

// ── Edit profile form ─────────────────────────────────────────────────────
const openEditForm = () => { showEditForm.value = true }
const closeEditForm= () => { showEditForm.value = false }

/**
 * Receives the updated user object payload from <EditProfile>
 * and replaces our local copy, then hides the modal.
 */
function handleProfileUpdated(updatedUser) {
  user.value = updatedUser
  showEditForm.value = false
}

// Load everything on mount
onMounted(async () => {
  await fetchUser()
  await fetchCustomOrders()
})
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
