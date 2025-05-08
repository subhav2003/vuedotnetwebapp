<template>
  <div class="flex min-h-screen">
    <!-- Sidebar Navigation -->
    <aside class="w-auto">
      <ArtisanNavBar />
    </aside>

    <!-- Main Content Area -->
    <main class="pt-20 px-4 md:px-8 flex-1 overflow-x-hidden">
      <!-- User Information Section -->
      <div class="rounded-lg p-6 bg-transparent">
        <UserInfo :user="user" @edit-profile="openEditUserForm" />
      </div>

      <!-- Artisan Profile Section -->
      <div class="rounded-lg p-6 bg-transparent">
        <ArtisanProfile
            :artisan="artisan"
            @edit-artisan="openEditArtisanForm"
        />
      </div>
    </main>
  </div>

  <!-- Edit User Form Overlay -->
  <div
      v-if="showEditUserForm"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
  >
    <EditUserForm
        :user="user"
        @close="closeEditUserForm"
        @updated="handleUserUpdated"
    />
  </div>

  <!-- Edit Artisan Profile Form Overlay -->
  <div
      v-if="showEditArtisanForm"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50"
  >
    <EditArtisanProfileForm
        @close="closeEditArtisanForm"
        @updated="handleArtisanUpdated"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import ArtisanNavBar from '@/components/ArtisansDashboard/ArtisanNavBar.vue';
import ArtisanProfile from '@/components/ArtisansDashboard/ArtisanProfile.vue';
import UserInfo from '@/components/UserInfo.vue';
import EditUserForm from '@/components/EditUserForm.vue';
import EditArtisanProfileForm from '@/components/ArtisansDashboard/EditArtisanProfileForm.vue';

const user = ref(null);
const artisan = ref(null);
const showEditUserForm = ref(false);
const showEditArtisanForm = ref(false);

// Fetch user data
const fetchUser = async () => {
  try {
    const token = localStorage.getItem('authToken');
    const res = await fetch(`${process.env.VUE_APP_API_URL}/api/v1/user`, {
      headers: { Authorization: `Bearer ${token}` }
    });
    user.value = await res.json();
  } catch (err) {
    console.error('Error fetching user:', err);
  }
};

// Fetch artisan data
const fetchArtisan = async () => {
  try {
    const token = localStorage.getItem('authToken');
    const res = await fetch(
        `${process.env.VUE_APP_API_URL}/api/v1/artisan/profileinfo`,
        { headers: { Authorization: `Bearer ${token}` } }
    );
    artisan.value = await res.json();
  } catch (err) {
    console.error('Error fetching artisan:', err);
  }
};

onMounted(() => {
  fetchUser();
  fetchArtisan();
});

// User edit overlay handlers
const openEditUserForm = () => {
  showEditUserForm.value = true;
};
const closeEditUserForm = () => {
  showEditUserForm.value = false;
};
const handleUserUpdated = (updatedUser) => {
  user.value = updatedUser;
  closeEditUserForm();
};

// Artisan edit overlay handlers
const openEditArtisanForm = () => {
  showEditArtisanForm.value = true;
};
// **Every time we close the artisan‐edit overlay, re‐fetch the profile**
const closeEditArtisanForm = () => {
  showEditArtisanForm.value = false;
  fetchArtisan();
};
const handleArtisanUpdated = (updatedArtisan) => {
  // Immediately update local copy
  artisan.value = updatedArtisan;
  // Then close (which will also trigger a fresh fetch)
  closeEditArtisanForm();
};
</script>

<style scoped>
.bg-transparent {
  background-color: transparent;
}
</style>
