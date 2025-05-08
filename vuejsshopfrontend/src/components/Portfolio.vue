<template>
  <!-- Toast stack (teleported to body) -->
  <Toast
      :toasts="toasts"
      @close="removeToast"
  />

  <!-- Overlay Container -->
  <div class="fixed inset-0 z-50 flex items-center justify-center">
    <div class="absolute inset-0 bg-black bg-opacity-50"></div>
    <div
        class="relative w-full max-w-4xl mx-auto p-6 rounded-lg shadow-lg overflow-y-auto max-h-[90vh] portfolio-bg"
    >
      <div class="relative z-10">

        <!-- Close Button -->
        <button
            @click="closePortfolio"
            class="absolute top-4 right-4 text-gray-500 hover:text-red-600 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none"
               viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                  d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>

        <!-- Loading Indicator -->
        <div v-if="loadingArtisan" class="flex justify-center items-center h-32">
          <div class="loader ease-linear rounded-full border-4 border-t-4 border-gray-200 h-12 w-12"></div>
        </div>

        <!-- Artisan Profile & Products -->
        <div v-else>
          <!-- Artisan Profile Header -->
          <div class="flex flex-col md:flex-row mb-6">
            <!-- Profile Picture with Fallback -->
            <div class="w-32 h-32 rounded-full overflow-hidden mr-6 flex-shrink-0">
              <img
                  :src="currentImageUrl"
                  alt="Profile Image"
                  class="object-cover w-full h-full"
                  @error="handleImageError"
              />
            </div>

            <!-- Artisan Info -->
            <div class="space-y-2 mt-4 md:mt-0 p-4 rounded-lg artisan-info-bg">
              <div class="flex items-center justify-between gap-4">
                <h2 class="text-3xl font-bold flex-1">
                  {{ artisan.name || 'Unnamed Artisan' }}
                </h2>
              </div>

              <p><strong>Bio:</strong> {{ artisan.bio || 'N/A' }}</p>
              <p><strong>Location:</strong> {{ artisan.location || 'N/A' }}</p>
              <p><strong>Skills:</strong> {{ artisan.skills || 'N/A' }}</p>
              <p>
                <strong>Lead Time:</strong>
                {{ artisan.lead_time !== null ? artisan.lead_time + ' days' : 'N/A' }}
              </p>
              <p><strong>Certifications:</strong> {{ artisan.certifications || 'N/A' }}</p>
              <p>
                <strong>Experience:</strong>
                {{ artisan.years_of_experience !== null ? artisan.years_of_experience + ' years' : 'N/A' }}
              </p>
              <p>
                <strong>Rating:</strong> {{ (artisan.average_rating ?? 0).toFixed(1) }}/5
              </p>
              <p>
                <strong>Portfolio:</strong>
                <span v-if="artisan.portfolio_url" class="underline">
                  <a :href="artisan.portfolio_url" target="_blank">
                    {{ artisan.portfolio_url }}
                  </a>
                </span>
                <span v-else>N/A</span>
              </p>

              <div>
                <strong>Social Links:</strong>
                <ul class="ml-4 list-disc">
                  <li>
                    Facebook:
                    <span v-if="parsedSocialLinks.facebook" class="underline ml-1">
                      <a :href="parsedSocialLinks.facebook" target="_blank">
                        {{ parsedSocialLinks.facebook }}
                      </a>
                    </span>
                    <span v-else class="ml-1">N/A</span>
                  </li>
                  <li>
                    Twitter:
                    <span v-if="parsedSocialLinks.twitter" class="underline ml-1">
                      <a :href="parsedSocialLinks.twitter" target="_blank">
                        {{ parsedSocialLinks.twitter }}
                      </a>
                    </span>
                    <span v-else class="ml-1">N/A</span>
                  </li>
                  <li>
                    Instagram:
                    <span v-if="parsedSocialLinks.instagram" class="underline ml-1">
                      <a :href="parsedSocialLinks.instagram" target="_blank">
                        {{ parsedSocialLinks.instagram }}
                      </a>
                    </span>
                    <span v-else class="ml-1">N/A</span>
                  </li>
                </ul>
              </div>

              <!-- Custom Order Status and Button -->
              <p class="mt-2">
                <strong>Accepts Custom Orders:</strong>
                <span class="ml-1">{{ artisan.accept_custom_order ? 'Yes' : 'No' }}</span>
              </p>
              <div class="flex justify-between items-center mt-4">
                <!-- Button only if authenticated & accepts orders -->
                <div v-if="isAuthenticated && artisan.accept_custom_order">
                  <button
                      class="bg-brown-500 text-white py-1 px-3 rounded text-sm hover:bg-amber-500 transition duration-300"
                      @click="openCustomOrderForm"
                  >
                    Initiate Custom Order
                  </button>
                </div>
                <!-- Flag slot remains unchanged -->
                <div>
                  <slot name="flag"/>
                </div>
              </div>
            </div>
          </div>

          <!-- Products Listing -->
          <div>
            <h3 class="text-2xl font-bold mb-4">
              Products by {{ artisan.name || 'N/A' }}
            </h3>
            <div v-if="loadingProducts" class="flex justify-center items-center h-64">
              <div class="loader ease-linear rounded-full border-4 border-t-4 border-gray-200 h-12 w-12"></div>
            </div>
            <div v-else-if="products.length === 0">No products available</div>
            <div
                class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6"
                v-else
            >
              <ProductCard
                  v-for="product in products"
                  :key="product.id"
                  :product="product"
                  @showProductDetail="openProductDetail"
              >
                <template #action-button>
                  <div class="flex justify-end items-center w-full py-2">
                    <AddToCartButton
                        :product="product"
                        @addToCart="handleAddToCart"
                    />
                  </div>
                </template>
              </ProductCard>
            </div>
          </div>

          <!-- Nested Product Detail Overlay -->
          <ProductDetail
              v-if="showProductDetail"
              :product="selectedProductDetail"
              @close="closeProductDetail"
          >
            <template #flag>
              <FlagButton
                  v-if="isAuthenticated"
                  type="product"
                  :id="selectedProductDetail.id"
              />
            </template>
            <template #add-to-cart>
              <div class="flex space-x-2 items-center">
                <AddToCartButton
                    :product="selectedProductDetail"
                    @addToCart="handleAddToCart"
                />
              </div>
            </template>
          </ProductDetail>
        </div>
      </div>
    </div>
  </div>

  <!-- Nested Custom Order Form Overlay -->
  <CustomOrderForm
      v-if="showCustomOrderForm"
      class="fixed inset-0 z-60 flex items-center justify-center bg-black bg-opacity-50"
      :artisanId="artisan.id"
      @close="closeCustomOrderForm"
      @order-submitted="handleOrderSubmitted"
  />
</template>

<script setup>
import { ref, reactive, onMounted, computed, watch } from 'vue';
import axios from 'axios';
import ProductCard from '@/components/ProductCard.vue';
import ProductDetail from '@/components/ProductDetail.vue';
import AddToCartButton from '@/components/AddToCartButton.vue';
import CustomOrderForm from '@/components/CustomOrderForm.vue';
import FlagButton from '@/components/FlagButton.vue';
import Toast from '@/components/Toast.vue';
import { useCart } from '@/store/useCart';

const props = defineProps({ artisanId: { type: Number, required: true } });
const emit = defineEmits(['close', 'custom-order']);

const apiBaseUrl = process.env.VUE_APP_API_URL;
const artisan = ref({});
const products = ref([]);
const loadingArtisan = ref(true);
const loadingProducts = ref(true);

const showProductDetail = ref(false);
const selectedProductDetail = ref(null);
const showCustomOrderForm = ref(false);
const isAuthenticated = !!localStorage.getItem('authToken');

const { addToCart } = useCart();

// Toast logic
const toasts = reactive([]);
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}

// Profile‐picture fallback
const currentImageUrl = ref('');
watch(artisan, (newArt) => {
  currentImageUrl.value = newArt.profile_image_url || getFallbackImage(newArt.name);
}, { immediate: true });

function handleImageError() {
  currentImageUrl.value = getFallbackImage(artisan.value.name);
}
function getFallbackImage(name) {
  const initials = encodeURIComponent(name || 'Artisan');
  return `https://ui-avatars.com/api/?name=${initials}&background=999&color=fff&size=256`;
}

// Fetch artisan & products
onMounted(() => {
  fetchArtisan();
  fetchProducts();
});
async function fetchArtisan() {
  try {
    const token = localStorage.getItem('authToken');
    const { data } = await axios.get(`${apiBaseUrl}/api/v1/artisan-profile`, {
      headers: { Authorization: `Bearer ${token}` },
      params: { artisan_id: props.artisanId },
    });
    artisan.value = data.data || data;
  } finally {
    loadingArtisan.value = false;
  }
}
async function fetchProducts() {
  try {
    const token = localStorage.getItem('authToken');
    const { data } = await axios.get(`${apiBaseUrl}/api/v1/artisan-products`, {
      headers: { Authorization: `Bearer ${token}` },
      params: { artisan_id: props.artisanId },
    });
    products.value = data.data;
  } finally {
    loadingProducts.value = false;
  }
}

// Overlay controls
function closePortfolio() {
  emit('close');
}
function openCustomOrderForm() {
  showCustomOrderForm.value = true;
  document.body.style.overflow = 'hidden';
}
function closeCustomOrderForm() {
  showCustomOrderForm.value = false;
  document.body.style.overflow = '';
}
function handleOrderSubmitted() {
  emit('custom-order', artisan.value.id);
}

// Product‐detail overlay
function openProductDetail(product) {
  selectedProductDetail.value = product;
  showProductDetail.value = true;
  document.body.style.overflow = 'hidden';
}
function closeProductDetail() {
  showProductDetail.value = false;
  selectedProductDetail.value = null;
  document.body.style.overflow = '';
}

// Add to cart
async function handleAddToCart({ product, quantity }) {
  const { success, message } = await addToCart(product, quantity);
  showToast(message, success ? 'success' : 'error');
}

// Social‐links parser
const parsedSocialLinks = computed(() => {
  if (artisan.value.social_links) {
    try { return JSON.parse(artisan.value.social_links); }
    catch {}
  }
  return {};
});
</script>

<style scoped>
.portfolio-bg {
  background-image: url('@/assets/portfolio.webp');
  background-position: center;
  background-size: cover;
}
.artisan-info-bg {
  background-color: rgba(255,255,255,0.9);
  color: #3B2712;
}
.loader {
  border-top-color: #3498db;
  animation: spin 1s ease-in-out infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }
.bg-brown-500 { background-color: #A0522D; }
.hover\:bg-brown-600:hover { background-color: #8B4513; }
.ring-brown-200 { --tw-ring-color: #d3b8ae; }
.max-h-\[90vh\] { max-height: 90vh; }
</style>
